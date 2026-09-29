namespace SpecFirst.Service.Tests.Spec;

/// <summary>
/// Links a test to the requirement it proves. The hash is the first eight hex characters of SHA-256 over the
/// normalised requirement text (trimmed, every run of whitespace collapsed to one space). The traceability guardrail
/// fails when the id is unknown or the hash no longer matches: a human then either confirms the test still proves the
/// reworded requirement and updates the hash, or marks the test work in progress with a Category=wip trait.
/// Feature files carry the same information as a tag: @REQ-nnn:hash.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public sealed class RequirementAttribute(string id, string hash) : Attribute
{
    public string Id { get; } = id;
    public string Hash { get; } = hash;
}
