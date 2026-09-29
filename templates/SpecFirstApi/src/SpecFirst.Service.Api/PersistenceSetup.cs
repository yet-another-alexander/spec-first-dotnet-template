using Microsoft.EntityFrameworkCore;

namespace SpecFirst.Service.Api;

public static class PersistenceSetup
{
    public static IServiceCollection AddPostgres(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options
                .UseNpgsql(configuration.GetConnectionString(ApiConstants.PostgresConnectionString))
                .UseSnakeCaseNamingConvention()
        );
        services.AddHealthChecks().AddDbContextCheck<AppDbContext>(ApiConstants.PostgresHealthCheck);
        return services;
    }
}
