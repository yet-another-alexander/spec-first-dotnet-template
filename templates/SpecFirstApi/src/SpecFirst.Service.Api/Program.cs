var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddJsonConsoleOutsideDevelopment(builder.Environment);

builder.Services.AddPostgres(builder.Configuration);
builder.Services.AddMessaging(builder.Configuration);
builder.Services.AddProblemDetailsWithHandler();
builder.Services.AddStrictJson();
builder.Services.AddValidation();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddOpenApiDocument();
builder.Services.AddTelemetry(builder.Configuration);

var app = builder.Build();

app.MigrateDatabase();
app.UseExceptionHandler();
app.UseStatusCodePages();

app.MapOpenApiReference();
app.MapHealthChecks(ApiConstants.HealthRoute);
app.MapOrders();

await app.RunAsync();

// Lets the test projects host the application through WebApplicationFactory<Program>.
public partial class Program
{
    protected Program() { }
}
