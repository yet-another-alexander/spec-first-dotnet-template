using System.Text.RegularExpressions;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// Requirements and tests must agree. Lifecycle of a requirement: no test (forbidden) -> work in progress (spec ready,
/// no code) -> covered. Only the last step happens in an implementation PR, and only by deleting the wip marker.
/// </summary>
[Collection(GuardrailsCollection.Name)]
public sealed partial class TraceabilityTests
{
    private const string ClarificationMarker = "[NEEDS CLARIFICATION";

    private static readonly IReadOnlyList<Requirement> Requirements = RequirementsFiles.ReadAll(
        RepoRoot.Resolve("docs/specs/requirements")
    );

    private static readonly TestScan Tests = TestMarkers.Scan(RepoRoot.Resolve("tests"));

    [GeneratedRegex(@"\bshall\b")]
    private static partial Regex Shall();

    [Fact]
    public void Every_requirement_is_one_sentence_with_one_shall()
    {
        var violations = Requirements
            .Where(requirement => Shall().Count(requirement.Text) != 1)
            .Select(requirement =>
                $"{requirement.Location}: {requirement.Id} must be one EARS sentence with exactly one 'shall'"
            )
            .ToList();

        violations.ShouldBeEmpty(string.Join('\n', violations));
    }

    [Fact]
    public void Requirement_ids_are_unique()
    {
        var duplicates = Requirements
            .GroupBy(requirement => requirement.Id)
            .Where(group => group.Count() > 1)
            .Select(group =>
                $"{group.Key} is defined {group.Count()} times: {string.Join(", ", group.Select(r => r.Location))}"
            )
            .ToList();

        duplicates.ShouldBeEmpty(string.Join('\n', duplicates));
    }

    [Fact]
    public void Every_testable_requirement_has_a_test()
    {
        var tested = Tests.Markers.Select(marker => marker.Id).ToHashSet(StringComparer.Ordinal);
        var untested = Requirements
            .Where(requirement => requirement.IsTestable && !tested.Contains(requirement.Id))
            .Select(requirement =>
                $"{requirement.Location}: {requirement.Id} has no test. Add a scenario tagged @{requirement.Id}:{requirement.Hash} "
                + "with @wip on the next line, or a fact with [Requirement] and a wip trait, or tag the requirement @no-test."
            )
            .ToList();

        untested.ShouldBeEmpty(string.Join('\n', untested));
    }

    [Fact]
    public void Every_test_marker_points_to_a_known_requirement()
    {
        var known = Requirements.Select(requirement => requirement.Id).ToHashSet(StringComparer.Ordinal);
        var unknown = Tests
            .Markers.Where(marker => !known.Contains(marker.Id))
            .Select(marker => $"{marker.Location}: {marker.Id} is not defined in docs/specs/requirements")
            .ToList();

        unknown.ShouldBeEmpty(string.Join('\n', unknown));
    }

    [Fact]
    public void Every_test_marker_carries_the_current_requirement_hash()
    {
        var hashes = Requirements.ToDictionary(
            requirement => requirement.Id,
            requirement => requirement.Hash,
            StringComparer.Ordinal
        );
        var stale = Tests
            .Markers.Where(marker => hashes.TryGetValue(marker.Id, out var current) && marker.Hash != current)
            .Select(marker =>
                marker.Hash is null
                    ? $"{marker.Location}: @{marker.Id} needs its hash: @{marker.Id}:{hashes[marker.Id]}"
                    : $"{marker.Location}: {marker.Id} changed since this test was written (hash {marker.Hash}, now {hashes[marker.Id]}). "
                        + "Confirm the test still proves the requirement and update the hash, or mark the test wip."
            )
            .ToList();

        stale.ShouldBeEmpty(string.Join('\n', stale));
    }

    [Fact]
    public void Wip_stands_alone_on_its_line() => Tests.Problems.ShouldBeEmpty(string.Join('\n', Tests.Problems));

    [Fact]
    public void The_spec_has_no_open_clarifications()
    {
        var open = Directory
            .EnumerateFiles(RepoRoot.Resolve("docs/specs"), "*.*", SearchOption.AllDirectories)
            .SelectMany(file =>
                File.ReadLines(file)
                    .Select((line, index) => (Line: line, Number: index + 1))
                    .Where(entry => entry.Line.Contains(ClarificationMarker, StringComparison.Ordinal))
                    .Select(entry => $"{RepoRoot.Relative(file)}:{entry.Number}: {entry.Line.Trim()}")
            )
            .ToList();

        open.ShouldBeEmpty(
            "The spec still has open questions; answer them before merging:\n" + string.Join('\n', open)
        );
    }
}
