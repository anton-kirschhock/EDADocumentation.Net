using System.Diagnostics.CodeAnalysis;

namespace EDADocumentation.Net.Models
{
    public class Process
    {
        public string Name { get; set; }

        public List<(string EventName, string Order)> Events { get; set; } = new List<(string EventName, string Order)>();

        public List<string> Services { get; set; } = new List<string>();

        public override bool Equals(object? obj)
        {
            return obj is Process process &&
                   Name == process.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public void AddEventData([NotNull] Event newEvent, string order = "99")
        {
            if (!Events.Any(e => e.EventName == newEvent.Name))
            {
                Events.Add((newEvent.Name, order));
            }

            if (newEvent.Producers?.Any() ?? false)
            {
                foreach (var svc in newEvent.Producers)
                {
                    if (!Services.Any(e => e == svc))
                        Services.Add(svc);
                }
            }

            if (newEvent.Consumers?.Any() ?? false)
            {
                foreach (var svc in newEvent.Consumers)
                {
                    if (!Services.Any(e => e == svc))
                        Services.Add(svc);
                }
            }
        }

        public async Task WriteTo(Stream stream)
        {
            const string indent = "  ";
            using var writer = new StreamWriter(stream);

            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync($"- name: {Name}");

            if (Services?.Any() ?? false)
            {
                await writer.WriteLineAsync("- Services: ");
                foreach (var services in Services)
                {
                    await writer.WriteLineAsync($"{indent}- {services}");
                }
            }

            if (Events?.Any() ?? false)
            {
                await writer.WriteLineAsync("- Events: ");
                foreach (var @event in Events.OrderBy(e => e.Order))
                {
                    await writer.WriteLineAsync($"{indent}- {@event.EventName}");
                }
            }


            await writer.WriteLineAsync("---");

            await writer.WriteLineAsync();

        }
    }
}
