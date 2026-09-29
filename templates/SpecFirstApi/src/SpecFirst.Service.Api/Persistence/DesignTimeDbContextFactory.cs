using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SpecFirst.Service.Api.Persistence;

/// <summary>
/// Used by `dotnet ef migrations add`. No database is contacted, but the same naming convention as at runtime must
/// apply or the model snapshot drifts from the running schema.
/// </summary>
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(
            new DbContextOptionsBuilder<AppDbContext>()
                .UseNpgsql("Host=localhost;Database=design-time")
                .UseSnakeCaseNamingConvention()
                .Options
        );
}
