using DotNetCore.CAP;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;
using Testcontainers.RabbitMq;

namespace SpecFirst.Service.Tests.Spec;

/// <summary>The API on a real Postgres and RabbitMQ. One container each per fixture; the migrations run when the host starts.</summary>
public sealed class ApiFixture : WebApplicationFactory<Program>, IAsyncLifetime
{
    private const string PostgresImage = "postgres:18-alpine";
    private const string RabbitMqImage = "rabbitmq:4-alpine";
    private const string Database = "specfirst";
    private const string User = "postgres";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder(PostgresImage)
        .WithDatabase(Database)
        .WithUsername(User)
        .WithPassword(User)
        .Build();

    private readonly RabbitMqContainer _rabbitMq = new RabbitMqBuilder(RabbitMqImage).Build();

    /// <summary>What the service published, as seen by a consumer on the broker.</summary>
    public MessageSink Messages => Services.GetServices<ICapSubscribe>().OfType<MessageSink>().Single();

    public async Task InitializeAsync()
    {
        await Task.WhenAll(_postgres.StartAsync(), _rabbitMq.StartAsync());
        _ = Server; // boots the host, which applies the migrations
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
        await _rabbitMq.DisposeAsync();
    }

    /// <summary>The same API and database with a clock that throws: the seam for provoking an unhandled exception.</summary>
    public WebApplicationFactory<Program> WithBrokenClock() =>
        WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services => services.AddSingleton<TimeProvider>(new BrokenClock()))
        );

    /// <summary>pg_dump --schema-only of the migrated database, reduced to the lines that describe the schema.</summary>
    public async Task<IReadOnlyList<string>> DumpSchemaAsync()
    {
        var dump = await _postgres.ExecAsync([
            "pg_dump",
            "--schema-only",
            // CAP owns the `cap` schema (outbox, inbox, lock); it is library storage, not the service's contract.
            "--exclude-schema",
            "cap",
            "--no-owner",
            "--no-privileges",
            "--no-comments",
            "--username",
            User,
            "--dbname",
            Database,
        ]);
        dump.ExitCode.ShouldBe(0, dump.Stderr);
        return SchemaDump.Normalise(dump.Stdout);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseSetting(
            $"ConnectionStrings:{ApiConstants.PostgresConnectionString}",
            _postgres.GetConnectionString()
        );
        builder.UseSetting($"{ApiConstants.RabbitMqSection}:HostName", _rabbitMq.Hostname);
        builder.UseSetting($"{ApiConstants.RabbitMqSection}:Port", _rabbitMq.GetMappedPublicPort(5672).ToString());
        builder.UseSetting($"{ApiConstants.RabbitMqSection}:UserName", "rabbitmq");
        builder.UseSetting($"{ApiConstants.RabbitMqSection}:Password", "rabbitmq");
        // Registered by type: CAP discovers subscribers from the service descriptors' implementation types.
        builder.ConfigureTestServices(services => services.AddSingleton<ICapSubscribe, MessageSink>());
    }

    private sealed class BrokenClock : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => throw new InvalidOperationException("the clock is broken");
    }
}
