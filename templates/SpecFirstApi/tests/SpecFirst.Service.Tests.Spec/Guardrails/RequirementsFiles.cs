using System.Text;
using System.Text.RegularExpressions;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// Reads requirements from the markdown files: a `### REQ-nnn` or `### TECH-nnn` heading with optional tags, then the
/// first paragraph after it as the requirement text. Everything after a blank line is commentary.
/// </summary>
public static partial class RequirementsFiles
{
    [GeneratedRegex(@"^###\s+(?<id>(?:REQ|TECH)-\d{3,})(?<tags>(?:\s+@[\w-]+)*)\s*$")]
    private static partial Regex Heading();

    public static IReadOnlyList<Requirement> ReadAll(string directory) =>
        Directory.EnumerateFiles(directory, "*.md").Order(StringComparer.Ordinal).SelectMany(Read).ToList();

    public static IReadOnlyList<Requirement> Read(string file)
    {
        var lines = File.ReadAllLines(file);
        var requirements = new List<Requirement>();

        for (var i = 0; i < lines.Length; i++)
        {
            var heading = Heading().Match(lines[i]);
            if (!heading.Success)
                continue;

            var tags = heading.Groups["tags"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var text = ParagraphAfter(lines, i);
            requirements.Add(
                new Requirement(heading.Groups["id"].Value, text, tags, $"{RepoRoot.Relative(file)}:{i + 1}")
            );
        }

        return requirements;
    }

    private static string ParagraphAfter(string[] lines, int headingIndex)
    {
        var text = new StringBuilder();
        var i = headingIndex + 1;
        while (i < lines.Length && string.IsNullOrWhiteSpace(lines[i]))
            i++;
        while (
            i < lines.Length
            && !string.IsNullOrWhiteSpace(lines[i])
            && !lines[i].StartsWith("###", StringComparison.Ordinal)
        )
            text.AppendLine(lines[i++]);
        return text.ToString();
    }
}
