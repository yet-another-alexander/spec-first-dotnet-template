using System.Net;
using System.Net.Http.Json;

namespace SpecFirst.Service.Tests.Spec.Integration;

[Collection(IntegrationCollection.Name)]
public sealed class ErrorHandlingTests(ApiFixture api)
{
    [Fact]
    [Requirement("TECH-003", "46791949")]
    public async Task An_unhandled_exception_becomes_a_problem_that_leaks_nothing()
    {
        using var faulty = api.WithBrokenClock();
        var request = new SubmitOrderRequest([new OrderLineRequest("APPLE", 1, 1.00m)]);

        var response = await faulty.CreateClient().PostAsJsonAsync("/v1/orders", request);

        response.StatusCode.ShouldBe(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");
        var body = await response.Content.ReadAsStringAsync();
        body.ShouldNotContain("the clock is broken");
        body.ShouldNotContain("InvalidOperationException");
    }
}
