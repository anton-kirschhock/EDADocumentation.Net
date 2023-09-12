namespace EDADocumentation.Net.Models
{
    public enum EventType
    {
        UserInteraction,
        SystemInteraction
    }
    public class Event
    {
        public EventType EventType { get; set; }
        public string? Key { get; set; }
        public string? Name { get; set; }

        public string? Summary { get; set; }
        public string? Description { get; set; }

        public List<string> Processes { get; set; } = new List<string>();
        public List<string> Producers { get; set; } = new List<string>();
        public List<string> Consumers { get; set; } = new List<string>();
        public List<string> PreceedingEvents { get; set; } = new List<string>();
        public List<string> SucceedingEvents { get; set; } = new List<string>();

        public override bool Equals(object? obj)
        {
            return obj is Event @event &&
                   EventType == @event.EventType &&
                   Key == @event.Key;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(EventType, Key);
        }

        public async Task WriteTo(Stream stream)
        {
            const string indent = "  ";
            using var writer = new StreamWriter(stream);

            await writer.WriteLineAsync("---");
            await writer.WriteLineAsync($"- name: {Name}");
            await writer.WriteAsync($"- summary:");
            await writer.WriteLineAsync(Summary);

            if (Producers?.Any() ?? false)
            {
                await writer.WriteLineAsync("- consumers: ");
                foreach (var consumer in Consumers)
                {
                    await writer.WriteLineAsync($"{indent}- {consumer}");
                }
            }

            if (Producers?.Any() ?? false)
            {
                await writer.WriteLineAsync("- producers: ");
                foreach (var producer in Producers)
                {
                    await writer.WriteLineAsync($"{indent}- {producer}");
                }
            }

            if (Processes?.Any() ?? false)
            {
                await writer.WriteLineAsync("- processes: ");
                foreach (var process in Processes)
                {
                    await writer.WriteLineAsync($"{indent}- {process}");
                }
            }

            if (PreceedingEvents?.Any() ?? false)
            {
                await writer.WriteLineAsync("- preceeding events: ");
                foreach (var pe in PreceedingEvents)
                {
                    await writer.WriteLineAsync($"{indent}- {pe}");
                }
            }

            if (SucceedingEvents?.Any() ?? false)
            {
                await writer.WriteLineAsync("- succeeding events: ");
                foreach (var se in SucceedingEvents)
                {
                    await writer.WriteLineAsync($"{indent}- {se}");
                }
            }


            await writer.WriteLineAsync("---");

            await writer.WriteLineAsync();

            if (!string.IsNullOrWhiteSpace(Description))
            {
                await writer.WriteLineAsync("# Description");
                await writer.WriteLineAsync(Description);
                await writer.WriteLineAsync();
            }
        }
    }
}
