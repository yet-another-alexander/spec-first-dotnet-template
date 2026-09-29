using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SpecFirst.Service.Api.Persistence;

namespace SpecFirst.Service.Tests.Spec.Integration;

[Collection(IntegrationCollection.Name)]
public sealed class MigrationsTests(ApiFixture api)
{
    [Fact]
    [Requirement("TECH-001", "034ed217")]
    public async Task All_migrations_are_applied_when_the_host_starts()
    {
        using var scope = api.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<AppDbContext>().Database;

        (await database.GetPendingMigrationsAsync()).ShouldBeEmpty();
        (await database.GetAppliedMigrationsAsync()).ShouldNotBeEmpty();
    }
}
