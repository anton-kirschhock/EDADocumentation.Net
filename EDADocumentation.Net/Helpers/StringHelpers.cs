using System.Xml.Linq;

namespace EDADocumentation.Net.Helpers
{
    public static class StringHelpers
    {
        /// <summary>
        /// Trims away whitespace, newlines and tabs
        /// </summary>
        /// <param name="s">string to trim</param>
        /// <returns>fully trimmed string</returns>
        public static string? FullTrim(this string? s)
            => string.Join('\n', s?.Trim()?.Trim('\n')?.Trim('\t').Split('\n').Select(e => e.Trim()));

        public static string? ParseEvent(this XElement element)
        {
            if (element.HasElements && element.Element("see") != null)
            {
                var cref = element.Element("see")?.Attribute("cref")?.Value.FullTrim();
                if (!string.IsNullOrWhiteSpace(cref))
                {
                    return cref[2..];
                }
            }
            else if (element.HasElements && element.Element("userInteraction") != null)
            {
                var userInteraction = element.Element("userInteraction")?.Value.FullTrim();
                if (!string.IsNullOrWhiteSpace(userInteraction))
                {
                    return userInteraction + "(user interaction)";
                }
            }
            // fallback - return the value and treat it as an userinteraction
            return element.Value + "(user interaction)";
        }
    }
}
