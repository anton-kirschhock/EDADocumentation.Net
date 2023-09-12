using System.Diagnostics.CodeAnalysis;

namespace EDADocumentation.Net.Models
{
    public class Service
    {
        public string Name { get; set; }

        public List<string> ConsumingEvents { get; set; } = new List<string>();
        public List<string> ProducingEvents { get; set; } = new List<string>();

        public void AddConsumingEvent([NotNull] Event newEvent)
        {
            if (!ConsumingEvents.Any(e => e == newEvent.Name))
            {
                ConsumingEvents.Add(newEvent.Name);
            }
        }

        public void AddProducingEvent([NotNull] Event newEvent)
        {
            if (!ProducingEvents.Any(e => e == newEvent.Name))
            {
                ProducingEvents.Add(newEvent.Name);
            }
        }


        public override bool Equals(object? obj)
        {
            return obj is Process process &&
                   Name == process.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name);
        }

        public async Task WriteTo(Stream stream)
        {
            const string indent = "  ";
            using var writer = new StreamWriter(stream);

            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync($"- name: {Name}");

            if (ConsumingEvents?.Any() ?? false)
            {
                await writer.WriteLineAsync("- consuming event: ");
                foreach (var consumingEvent in ConsumingEvents)
                {
                    await writer.WriteLineAsync($"{indent}- {consumingEvent}");
                }
            }

            if (ProducingEvents?.Any() ?? false)
            {
                await writer.WriteLineAsync("- producing events: ");
                foreach (var producingEvent in ProducingEvents)
                {
                    await writer.WriteLineAsync($"{indent}- {producingEvent}");
                }
            }


            await writer.WriteLineAsync("---");

            await writer.WriteLineAsync();

        }
    }
}
