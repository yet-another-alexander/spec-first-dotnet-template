using Microsoft.OpenApi;
using Scalar.AspNetCore;

namespace SpecFirst.Service.Api;

public static class OpenApiSetup
{
    public static IServiceCollection AddOpenApiDocument(this IServiceCollection services) =>
        services.AddOpenApi(options =>
        {
            // Enums serialise as strings (see the Models project), but the exporter omits the type the contract rules demand.
            options.AddSchemaTransformer(
                (schema, _, _) =>
                {
                    if (schema.Enum is { Count: > 0 })
                        schema.Type = JsonSchemaType.String;
                    return Task.CompletedTask;
                }
            );
            options.AddDocumentTransformer(
                (document, _, _) =>
                {
                    document.Info = new OpenApiInfo
                    {
                        Title = "SpecFirst.Service API",
                        Version = "v1",
                        Description =
                            "Generated from code. The contract of record is docs/specs/api/openapi.yaml; "
                            + "CI fails when this document exposes anything the contract does not declare.",
                    };
                    document.Tags = new HashSet<OpenApiTag>
                    {
                        new()
                        {
                            Name = "Orders",
                            Description = "Submitting, reading, confirming and cancelling orders",
                        },
                    };
                    return Task.CompletedTask;
                }
            );
        });

    /// <summary>The live document and the Scalar reference are for developers; they are served in Development only.</summary>
    public static void MapOpenApiReference(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
            return;

        app.MapOpenApi();
        app.MapScalarApiReference();
    }
}
