namespace SpecFirst.Service.Tests.Spec.Guardrails;

[Collection(GuardrailsCollection.Name)]
public sealed class EarsGrammarTests
{
    private static readonly IReadOnlyList<Requirement> Requirements = RequirementsFiles.ReadAll(
        RepoRoot.Resolve("docs/specs/requirements")
    );

    [Fact]
    public void Every_requirement_follows_an_EARS_pattern()
    {
        var violations = Requirements
            .Select(requirement => (requirement, violation: EarsGrammar.Violation(requirement.Text)))
            .Where(entry => entry.violation is not null)
            .Select(entry => $"{entry.requirement.Location}: {entry.requirement.Id} {entry.violation}")
            .ToList();

        violations.ShouldBeEmpty(string.Join('\n', violations));
    }

    [Theory]
    [InlineData("The system shall compute the total of an order.")]
    [InlineData("When a client submits an order, the system shall store it and respond 201.")]
    [InlineData("While an order is in status Confirmed, the system shall respond 204 to a confirmation request.")]
    [InlineData("If a client requests an order id that does not exist, then the system shall respond 404.")]
    [InlineData("Where multi-currency is enabled, the system shall convert the total to the tenant currency.")]
    [InlineData(
        "Where multi-currency is enabled, while an order is Submitted, when a line is added, the system shall recompute the total."
    )]
    [InlineData("If an exception occurs while handling a request, then the system shall respond 500.")]
    public void Well_formed_sentences_pass(string text) => EarsGrammar.Violation(text).ShouldBeNull();

    [Theory]
    [InlineData("The order total is the sum of its lines.", "shall")]
    [InlineData("the system shall compute the total.", "capital")]
    [InlineData("The system shall compute the total", "period")]
    [InlineData("The system shall compute the total. It shall also round it.", "one sentence")]
    [InlineData("The service shall compute the total.", "one of")]
    [InlineData("When a client submits an order the system shall store it.", "one of")]
    [InlineData("If a client submits no lines, the system shall respond 400.", "one of")]
    [InlineData("When an order is confirmed, while it is Submitted, the system shall publish an event.", "one of")]
    public void Malformed_sentences_are_named(string text, string expectedWord) =>
        EarsGrammar.Violation(text).ShouldNotBeNull().ShouldContain(expectedWord);
}
