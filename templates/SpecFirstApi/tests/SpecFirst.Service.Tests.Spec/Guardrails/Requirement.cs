namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>One requirement as read from docs/specs/requirements. Location is file:line of its heading.</summary>
public sealed record Requirement(string Id, string Text, IReadOnlyList<string> Tags, string Location)
{
    public bool IsTestable => !Tags.Contains("@no-test");

    public string Hash => RequirementHash.Of(Text);
}
