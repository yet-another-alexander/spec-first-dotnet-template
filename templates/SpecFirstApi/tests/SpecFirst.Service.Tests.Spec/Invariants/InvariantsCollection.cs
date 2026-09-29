namespace SpecFirst.Service.Tests.Spec.Invariants;

/// <summary>The invariants are property tests over pure functions; no host, no Docker.</summary>
[CollectionDefinition(Name)]
public sealed class InvariantsCollection
{
    public const string Name = "invariants";

    private InvariantsCollection() { }
}
