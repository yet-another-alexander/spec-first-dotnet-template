namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>Everything the scan of the test sources found: the markers, and the structural problems on the way.</summary>
public sealed record TestScan(IReadOnlyList<TestMarker> Markers, IReadOnlyList<string> Problems);
