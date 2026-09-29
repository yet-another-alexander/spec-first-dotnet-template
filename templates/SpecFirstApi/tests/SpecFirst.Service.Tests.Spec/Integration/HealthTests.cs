using System.Net;

namespace SpecFirst.Service.Tests.Spec.Integration;

[Collection(IntegrationCollection.Name)]
public sealed class HealthTests(ApiFixture api)
{
    [Fact]
    [Requirement("TECH-002", "1b3c8493")]
    public async Task Health_is_200_while_the_database_is_reachable()
    {
        var response = await api.CreateClient().GetAsync(ApiConstants.HealthRoute);

        response.StatusCode.ShouldBe(HttpStatusCode.OK);
        (await response.Content.ReadAsStringAsync()).ShouldBe("Healthy");
    }
}
