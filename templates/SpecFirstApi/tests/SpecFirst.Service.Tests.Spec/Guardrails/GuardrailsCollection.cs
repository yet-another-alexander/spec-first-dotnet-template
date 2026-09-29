namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>The guardrails read the repository and the compiled API; no host, no Docker.</summary>
[CollectionDefinition(Name)]
public sealed class GuardrailsCollection
{
    public const string Name = "guardrails";

    private GuardrailsCollection() { }
}
