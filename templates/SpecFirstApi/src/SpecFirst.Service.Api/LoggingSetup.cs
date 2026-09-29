namespace SpecFirst.Service.Api;

public static class LoggingSetup
{
    /// <summary>Structured JSON on stdout outside Development, ready for any log collector.</summary>
    public static ILoggingBuilder AddJsonConsoleOutsideDevelopment(
        this ILoggingBuilder logging,
        IHostEnvironment environment
    )
    {
        if (environment.IsDevelopment())
            return logging;

        logging.ClearProviders();
        return logging.AddJsonConsole(options =>
        {
            options.IncludeScopes = true;
            options.UseUtcTimestamp = true;
            options.TimestampFormat = "O";
        });
    }
}
