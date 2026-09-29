using DotNetCore.CAP;
using SpecFirst.Service.Api.Persistence;

namespace SpecFirst.Service.Api;

public static class MessagingSetup
{
    /// <summary>
    /// CAP with the transactional outbox in the service's own Postgres (schema `cap`, created by CAP itself) and
    /// RabbitMQ as transport. Publishing inside an EF transaction writes the outbox row with the business change; a
    /// background relay delivers it to the broker, so a message is never lost and never sent for a rolled-back change.
    /// The contract of what is published is docs/specs/messages/asyncapi.yaml.
    /// </summary>
    public static IServiceCollection AddMessaging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCap(options =>
        {
            options.UseEntityFramework<AppDbContext>();
            options.UseRabbitMQ(rabbit => configuration.GetSection(ApiConstants.RabbitMqSection).Bind(rabbit));
        });
        return services;
    }
}
