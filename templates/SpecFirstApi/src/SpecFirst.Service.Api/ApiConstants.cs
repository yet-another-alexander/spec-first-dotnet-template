namespace SpecFirst.Service.Api;

public static class ApiConstants
{
    public const string PostgresConnectionString = "Postgres";
    public const string PostgresHealthCheck = "postgres";
    public const string HealthRoute = "/health";
    public const string OtlpEndpointSetting = "OTEL_EXPORTER_OTLP_ENDPOINT";
    public const string RabbitMqSection = "RabbitMq";

    /// <summary>The entry assembly when the Release build generates the OpenAPI document: no database exists then.</summary>
    public const string SpecGenerationEntryAssembly = "GetDocument.Insider";
}
