namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>A reference from a test to a requirement: a feature tag or a Requirement attribute. Hash is null when the tag has none.</summary>
public sealed record TestMarker(string Id, string? Hash, bool Wip, string Location);
