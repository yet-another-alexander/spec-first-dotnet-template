namespace SpecFirst.Service.Tests.Spec.Integration;

/// <summary>
/// The integration tests run one at a time against one API on its own Postgres and RabbitMQ, started once for the
/// collection. The acceptance scenarios have their own host (see Acceptance/ApiHooks.cs).
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationCollection : ICollectionFixture<ApiFixture>
{
    public const string Name = "integration";
}
