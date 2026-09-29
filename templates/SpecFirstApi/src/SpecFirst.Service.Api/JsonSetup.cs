using System.Text.Json.Serialization;

namespace SpecFirst.Service.Api;

public static class JsonSetup
{
    /// <summary>
    /// Numbers are numbers. Without this, the default of reading numbers from strings leaks `number | string` into every
    /// numeric field of the generated OpenAPI document.
    /// </summary>
    public static IServiceCollection AddStrictJson(this IServiceCollection services) =>
        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.NumberHandling = JsonNumberHandling.Strict
        );
}
