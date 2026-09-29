using Reqnroll;

namespace SpecFirst.Service.Tests.Spec.Acceptance;

/// <summary>
/// Reqnroll has no collection fixtures: the API starts once, before the first scenario, and every scenario shares it.
/// Starting lazily keeps Docker out of runs that select no scenario, such as the guardrails alone.
/// </summary>
[Binding]
public sealed class ApiHooks
{
    private static readonly Lazy<Task<ApiFixture>> Started = new(StartAsync);

    private ApiHooks() { }

    public static ApiFixture Api => Started.Value.GetAwaiter().GetResult();

    [BeforeScenario]
    public static Task StartApi() => Started.Value;

    [AfterTestRun]
    public static async Task StopApi()
    {
        if (Started.IsValueCreated)
            await ((IAsyncLifetime)await Started.Value).DisposeAsync();
    }

    private static async Task<ApiFixture> StartAsync()
    {
        var api = new ApiFixture();
        await api.InitializeAsync();
        return api;
    }
}
