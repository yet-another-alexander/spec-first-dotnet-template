namespace SpecFirst.Service.Tests.Spec.Integration;

/// <summary>
/// Contract drift, database side. The check is one-directional: every statement the migrations produce must be in
/// docs/specs/db/schema.sql. Statements in the contract that the code does not produce yet are pending work, not drift.
/// </summary>
[Collection(IntegrationCollection.Name)]
public sealed class DatabaseSchemaTests(ApiFixture api)
{
    private const string ContractPath = "docs/specs/db/schema.sql";

    [Fact]
    public async Task Everything_in_the_database_schema_is_in_the_contract()
    {
        var actual = await api.DumpSchemaAsync();
        var contract = RepoRoot.Resolve(ContractPath);

        if (Environment.GetEnvironmentVariable("UPDATE_SCHEMA") == "1")
        {
            await File.WriteAllLinesAsync(contract, actual);
            return;
        }

        var declared = SchemaDump.Normalise(await File.ReadAllTextAsync(contract)).ToHashSet(StringComparer.Ordinal);
        var drift = actual.Where(line => !declared.Contains(line)).ToList();

        drift.ShouldBeEmpty(
            $"The database has schema statements that {ContractPath} does not declare. Add them to the contract in a spec PR, "
                + $"or run this test with UPDATE_SCHEMA=1 to regenerate the contract from the migrations:\n"
                + string.Join('\n', drift)
        );
    }
}
