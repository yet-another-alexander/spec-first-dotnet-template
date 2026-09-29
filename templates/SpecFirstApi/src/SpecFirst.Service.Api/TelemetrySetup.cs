using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SpecFirst.Service.Api;

public static class TelemetrySetup
{
    /// <summary>
    /// Traces and metrics from the first commit. They are exported over OTLP only when OTEL_EXPORTER_OTLP_ENDPOINT is
    /// set, so local runs and tests stay quiet.
    /// </summary>
    public static IServiceCollection AddTelemetry(this IServiceCollection services, IConfiguration configuration)
    {
        var telemetry = services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("SpecFirst.Service"))
            .WithTracing(tracing => tracing.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics.AddAspNetCoreInstrumentation().AddHttpClientInstrumentation());

        if (!string.IsNullOrEmpty(configuration[ApiConstants.OtlpEndpointSetting]))
            telemetry.UseOtlpExporter();

        return services;
    }
}
