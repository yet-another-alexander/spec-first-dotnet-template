using System.Net;
using System.Net.Http.Json;
using SpecFirst.Service.Models.Requests;
using SpecFirst.Service.Models.Responses;

namespace SpecFirst.Service.Tests.Spec.Integration;

[Collection(IntegrationCollection.Name)]
public sealed class MessagingTests(ApiFixture api)
{
    private static readonly TimeSpan Delivery = TimeSpan.FromSeconds(10);

    [Fact]
    [Requirement("TECH-005", "d7be59b8")]
    public async Task Confirming_an_order_publishes_order_confirmed()
    {
        var client = api.CreateClient();
        var orderId = await SubmitOrder(client);

        var response = await client.PostAsync($"/v1/orders/{orderId}/confirm", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        var published = await api.Messages.OrderConfirmedFor(orderId, Delivery);
        published.Count.ShouldBe(1);
        published[0].OrderId.ShouldBe(orderId);
    }

    [Fact]
    [Requirement("TECH-006", "a4d10ace")]
    public async Task Confirming_a_confirmed_order_publishes_nothing()
    {
        var client = api.CreateClient();
        var orderId = await SubmitOrder(client);
        await client.PostAsync($"/v1/orders/{orderId}/confirm", null);
        (await api.Messages.OrderConfirmedFor(orderId, Delivery)).Count.ShouldBe(1);

        var response = await client.PostAsync($"/v1/orders/{orderId}/confirm", null);

        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
        (await api.Messages.OrderConfirmedFor(orderId, Delivery)).Count.ShouldBe(1);
    }

    [Fact]
    [Requirement("TECH-006", "a4d10ace")]
    public async Task Concurrent_confirmations_of_one_order_publish_one_message()
    {
        var client = api.CreateClient();
        var orderId = await SubmitOrder(client);

        var responses = await Task.WhenAll(
            Enumerable.Range(0, 8).Select(_ => client.PostAsync($"/v1/orders/{orderId}/confirm", null))
        );

        responses.Select(r => r.StatusCode).ShouldAllBe(status => status == HttpStatusCode.NoContent);
        (await api.Messages.OrderConfirmedFor(orderId, Delivery)).Count.ShouldBe(1);
    }

    private static async Task<Guid> SubmitOrder(HttpClient client)
    {
        var request = new SubmitOrderRequest([new OrderLineRequest("APPLE", 1, 1.00m)]);
        var response = await client.PostAsJsonAsync("/v1/orders", request);
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        return (await response.Content.ReadFromJsonAsync<OrderResponse>())!.Id;
    }
}
