namespace SpecFirst.Service.Tests.Spec.Acceptance;

/// <summary>
/// The acceptance scenarios run one at a time against the host ApiHooks starts. Reqnroll generates each feature as a
/// partial class without a collection; a partial declaration next to the feature file adds it to this one.
/// </summary>
[CollectionDefinition(Name)]
public sealed class AcceptanceCollection
{
    public const string Name = "acceptance";

    private AcceptanceCollection() { }
}
