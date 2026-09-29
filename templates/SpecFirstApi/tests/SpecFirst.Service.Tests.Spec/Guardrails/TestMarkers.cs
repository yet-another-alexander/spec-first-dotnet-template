using System.Text.RegularExpressions;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// Finds requirement markers in the test sources. Feature files: tags of the form @REQ-nnn:hash on a Feature or a
/// Scenario, with @wip alone on a line marking work in progress. C# files: [Requirement("TECH-nnn", "hash")] on a test,
/// with [Trait("Category", "wip")] in the same attribute block marking work in progress.
/// </summary>
public static partial class TestMarkers
{
    private const string WipTag = "@wip";

    [GeneratedRegex(@"^@(?<id>(?:REQ|TECH)-\d+)(?::(?<hash>[0-9a-f]{8}))?$")]
    private static partial Regex RequirementTag();

    [GeneratedRegex(@"\[Requirement\(""(?<id>(?:REQ|TECH)-\d+)"",\s*""(?<hash>[0-9a-f]{8})""\)\]")]
    private static partial Regex RequirementAttribute();

    [GeneratedRegex(@"Trait\(\s*""Category""\s*,\s*""wip""\s*\)")]
    private static partial Regex WipTrait();

    public static TestScan Scan(string testsDirectory)
    {
        var markers = new List<TestMarker>();
        var problems = new List<string>();

        foreach (var file in SourceFiles(testsDirectory))
        {
            if (file.EndsWith(".feature", StringComparison.Ordinal))
                ScanFeature(file, markers, problems);
            else
                ScanCSharp(file, markers);
        }

        return new TestScan(markers, problems);
    }

    private static IEnumerable<string> SourceFiles(string root) =>
        Directory
            .EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(file =>
                file.EndsWith(".feature", StringComparison.Ordinal) || file.EndsWith(".cs", StringComparison.Ordinal)
            )
            .Where(file => !file.EndsWith(".feature.cs", StringComparison.Ordinal))
            .Where(file => !file.Split(Path.DirectorySeparatorChar).Any(part => part is "bin" or "obj"))
            .Order(StringComparer.Ordinal);

    private static void ScanFeature(string file, List<TestMarker> markers, List<string> problems)
    {
        var lines = File.ReadAllLines(file);
        var featureTags = new List<(string Tag, int Line)>();
        var pendingTags = new List<(string Tag, int Line)>();

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            var lineNumber = i + 1;

            if (line.StartsWith('@'))
            {
                if (line.Contains(WipTag, StringComparison.Ordinal) && line != WipTag)
                    problems.Add($"{RepoRoot.Relative(file)}:{lineNumber}: {WipTag} must be alone on its line");
                pendingTags.AddRange(
                    line.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(tag => (tag, lineNumber))
                );
            }
            else if (line.StartsWith("Feature:", StringComparison.Ordinal))
            {
                featureTags = pendingTags;
                pendingTags = [];
            }
            else if (
                line.StartsWith("Scenario", StringComparison.Ordinal)
                || line.StartsWith("Example:", StringComparison.Ordinal)
            )
            {
                var tags = featureTags.Concat(pendingTags).ToList();
                var wip = tags.Any(tag => tag.Tag == WipTag);
                foreach (var (tag, tagLine) in tags)
                {
                    var match = RequirementTag().Match(tag);
                    if (match.Success)
                    {
                        var hash = match.Groups["hash"].Success ? match.Groups["hash"].Value : null;
                        markers.Add(
                            new TestMarker(match.Groups["id"].Value, hash, wip, $"{RepoRoot.Relative(file)}:{tagLine}")
                        );
                    }
                }
                pendingTags = [];
            }
            else if (
                line.StartsWith("Examples:", StringComparison.Ordinal)
                || line.StartsWith("Rule:", StringComparison.Ordinal)
            )
            {
                pendingTags = [];
            }
        }
    }

    private static void ScanCSharp(string file, List<TestMarker> markers)
    {
        var lines = File.ReadAllLines(file);
        var attributeBlock = new List<(string Line, int Number)>();

        // The loop runs one past the end so the last block is flushed.
        for (var i = 0; i <= lines.Length; i++)
        {
            var line = i < lines.Length ? lines[i].Trim() : string.Empty;
            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                attributeBlock.Add((line, i + 1));
                continue;
            }

            if (attributeBlock.Count == 0)
                continue;

            var wip = attributeBlock.Any(attribute => WipTrait().IsMatch(attribute.Line));
            foreach (var (attribute, number) in attributeBlock)
            {
                foreach (Match match in RequirementAttribute().Matches(attribute))
                    markers.Add(
                        new TestMarker(
                            match.Groups["id"].Value,
                            match.Groups["hash"].Value,
                            wip,
                            $"{RepoRoot.Relative(file)}:{number}"
                        )
                    );
            }
            attributeBlock.Clear();
        }
    }
}
