using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

using EDADocumentation.Net.Helpers;
using EDADocumentation.Net.Models;

using Spectre.Console;
using Spectre.Console.Cli;

namespace EDADocumentation.Net.Commands
{
    public class ParseDocsCommandSettings : CommandSettings
    {
        [CommandArgument(0, "[WORKINGDIRECTORY]")]
        [DefaultValue(".")]
        public string RootDirectory { get; set; }

        [CommandArgument(1, "[OUTDIRECTORY]")]
        [DefaultValue("./docs")]
        public string OutDirectory { get; set; }

        public override ValidationResult Validate()
        {
            if (string.IsNullOrWhiteSpace(RootDirectory))
                return ValidationResult.Error("You must specify the Root Directory where I can scan all Documentation XML files from!");

            if (RootDirectory != "." && !Directory.Exists(RootDirectory))
                return ValidationResult.Error("The Root Directory you've specified does not exists or is no directory!");

            if (RootDirectory == ".")
            {
                RootDirectory = Directory.GetCurrentDirectory();
            }

            if (string.IsNullOrWhiteSpace(OutDirectory))
                return ValidationResult.Error("You must specify the Out Directory where I can write all doc files to!");

            if (!OutDirectory.StartsWith(".") && !Directory.Exists(OutDirectory))
                return ValidationResult.Error("The Out Directory you've specified does not exists or is no directory!");

            if (OutDirectory.StartsWith("."))
            {
                OutDirectory = Path.Combine(Directory.GetCurrentDirectory(), OutDirectory.Substring(1).TrimStart('/'));
            }

            return ValidationResult.Success();
        }
    }

    public record SelectedFilesPromptItem(string Name, string FullName);
    public class ParseDocsCommand : Command<ParseDocsCommandSettings>
    {
        public override int Execute([NotNull] CommandContext context, [NotNull] ParseDocsCommandSettings settings)
        {
            AnsiConsole.WriteLine("Generate Markdown documentation for your .Net Class Library XML documentation for your EDA!");
            var outDir = new DirectoryInfo(settings.OutDirectory);
            if (outDir.Exists && outDir.EnumerateFiles().Any())
            {
                AnsiConsole.WriteLine("I've noticed there are files in the OutDirectory");
                if (!AnsiConsole.Confirm("Are you sure you would like to continue? Files might be replaced with newer content"))
                {
                    AnsiConsole.WriteLine("I understand - Stopping!");
                    return 0;
                }
            }

            // Select all files
            var files = new DirectoryInfo(settings.RootDirectory).EnumerateFiles("*.xml", SearchOption.AllDirectories);
            var selectedFilesPrompt =
                    new MultiSelectionPrompt<SelectedFilesPromptItem>()
                        .Title("Select all [green]xml files[/] which should be included?")
                        .Required()
                        .PageSize(10)
                        .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                        .UseConverter(e => e.Name)
                        .InstructionsText(
                            "[grey](Press [blue]<space>[/] to toggle a file, " +
                            "[green]<enter>[/] to accept)[/]"
                        );

            files.GroupBy(e => e.DirectoryName).ToList().ForEach(group =>
            {
                selectedFilesPrompt.AddChoiceGroup(new SelectedFilesPromptItem(group.Key ?? "Others", null), group.Select(e => new SelectedFilesPromptItem(e.Name, e.FullName)));
            });

            var selectedFiles = AnsiConsole.Prompt(selectedFilesPrompt);
            AnsiConsole.Status().StartAsync($"Starting the processing of {selectedFiles.Count} xml files...", async (ctx) =>
            {
                var processes = new HashSet<Process>();
                var services = new HashSet<Service>();
                var events = new HashSet<Event>();
                // Each XML
                foreach (var file in selectedFiles)
                {
                    ctx.Status($"Parsing {file.FullName}");
                    var doc = XElement.Load(file.FullName);
                    var typeMembers = doc.Element("members")?
                                         .Elements("member")?
                                         .Where(e => e != null && e.Attribute("name") != null && e.Attribute("name")!.Value.StartsWith("T:"));


                    foreach (var item in typeMembers)
                    {
                        var typeName = item.Attribute("name")!.Value.Substring(2);
                        if (typeName.EndsWith("Event") || item.Element("isEvent") != null)
                        {
                            ctx.Status($"Parsing {file.FullName} - Processing type {typeName}...");
                            var evProcesses = item.Elements("process")
                                                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Value))
                                                .Select(e => (Name: e.Value.FullTrim(), Order: e.Attribute("eventOrder")?.Value ?? null))
                                                .Where(e => !string.IsNullOrWhiteSpace(e.Name));

                            var newEvent = new Event
                            {
                                Key = typeName,
                                Name = typeName.Substring(typeName.LastIndexOf(".") + 1),
                                Summary = item.Element("summary")?.Value.FullTrim(),
                                Description = item.Element("description")?.Value.FullTrim(),
                                Producers = item.Elements("producer")
                                                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Value))
                                                .Select(e => e.Value.FullTrim())
                                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                                .ToList()!,
                                Consumers = item.Elements("consumer")
                                                .Where(e => e != null && !string.IsNullOrWhiteSpace(e.Value))
                                                .Select(e => e.Value.FullTrim())
                                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                                .ToList()!,
                                Processes = evProcesses.Select(e => e.Name).ToList(),

                                PreceedingEvents = item.Elements("preceedingEvent")
                                                .Where(e => e != null)
                                                .Select(e => e.ParseEvent())
                                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                                .ToList()!,
                                SucceedingEvents = item.Elements("succeedingEvent")
                                                .Where(e => e != null)
                                                .Select(e => e.ParseEvent())
                                                .Where(e => !string.IsNullOrWhiteSpace(e))
                                                .ToList()!
                            };

                            events.Add(newEvent);

                            if (evProcesses?.Any() ?? false)
                            {
                                foreach (var proc in evProcesses)
                                {
                                    var process = processes.FirstOrDefault(e => e.Name == proc.Name);
                                    if (process == null)
                                    {
                                        process = new Process()
                                        {
                                            Name = proc.Name
                                        };
                                        processes.Add(process);
                                    }

                                    process.AddEventData(newEvent, proc.Order);
                                }
                            }

                            if (newEvent.Consumers?.Any() ?? false)
                            {
                                foreach (var consumerName in newEvent.Consumers)
                                {
                                    var consumer = services.FirstOrDefault(e => e.Name == consumerName);
                                    if (consumer == null)
                                    {
                                        consumer = new Service()
                                        {
                                            Name = consumerName
                                        };
                                        services.Add(consumer);
                                    }

                                    consumer.AddConsumingEvent(newEvent);
                                }
                            }

                            if (newEvent.Producers?.Any() ?? false)
                            {
                                foreach (var producerName in newEvent.Producers)
                                {
                                    var producer = services.FirstOrDefault(e => e.Name == producerName);
                                    if (producer == null)
                                    {
                                        producer = new Service()
                                        {
                                            Name = producerName
                                        };
                                        services.Add(producer);
                                    }

                                    producer.AddProducingEvent(newEvent);
                                }
                            }
                        }


                    }
                    ctx.Status($"Parsing {file.FullName}");

                }

                ctx.Status($"Writing {events.Count} events");
                var eventRoot = Path.Combine(settings.OutDirectory, "events");
                if (!Directory.Exists(eventRoot))
                {
                    Directory.CreateDirectory(eventRoot);
                }
                foreach (var e in events)
                {
                    ctx.Status($"Writing {events.Count} events - {e.Name}");

                    var path = Path.Combine(eventRoot, $"{e.Key}.md");
                    if (Path.Exists(path))
                        File.Delete(path);

                    await e.WriteTo(File.Create(path));
                }

                ctx.Status($"Writing {services.Count} services...");
                var svcRoot = Path.Combine(settings.OutDirectory, "services");
                if (!Directory.Exists(svcRoot))
                {
                    Directory.CreateDirectory(svcRoot);
                }
                foreach (var e in services)
                {
                    ctx.Status($"Writing {services.Count} services - {e.Name}");

                    var path = Path.Combine(svcRoot, $"{e.Name}.md");
                    if (Path.Exists(path))
                        File.Delete(path);

                    await e.WriteTo(File.Create(path));
                }

                ctx.Status($"Writing {processes.Count} processes...");
                var procRoot = Path.Combine(settings.OutDirectory, "processes");
                if (!Directory.Exists(procRoot))
                {
                    Directory.CreateDirectory(procRoot);
                }
                foreach (var e in processes)
                {
                    ctx.Status($"Writing {services.Count} processes - {e.Name}");

                    var path = Path.Combine(procRoot, $"{e.Name}.md");
                    if (Path.Exists(path))
                        File.Delete(path);

                    await e.WriteTo(File.Create(path));
                }

                ctx.Status("Cleaning up...");

                return Task.CompletedTask;
            });

            return 0;
        }
    }
}
