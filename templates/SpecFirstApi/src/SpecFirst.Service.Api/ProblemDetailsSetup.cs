namespace SpecFirst.Service.Api;

public static class ProblemDetailsSetup
{
    /// <summary>Every error response is application/problem+json, including unhandled exceptions.</summary>
    public static IServiceCollection AddProblemDetailsWithHandler(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        return services;
    }
}
