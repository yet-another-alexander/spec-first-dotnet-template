using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace SpecFirst.Service.Api;

public static class DatabaseBootstrap
{
    /// <summary>
    /// Applies pending migrations before the first request is served (TECH-001). Skipped when the build-time OpenAPI
    /// generator boots the host, because there is no database in that context.
    /// </summary>
    public static void MigrateDatabase(this WebApplication app)
    {
        if (Assembly.GetEntryAssembly()?.GetName().Name == ApiConstants.SpecGenerationEntryAssembly)
            return;

        using var scope = app.Services.CreateScope();
        scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
    }
}
