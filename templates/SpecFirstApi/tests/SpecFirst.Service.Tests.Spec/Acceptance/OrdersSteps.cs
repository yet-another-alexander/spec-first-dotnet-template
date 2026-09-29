using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Reqnroll;
using SpecFirst.Service.Models.Requests;
using SpecFirst.Service.Models.Responses;

namespace SpecFirst.Service.Tests.Spec.Acceptance;

[Binding]
public sealed class OrdersSteps
{
    private readonly HttpClient _client = ApiHooks.Api.CreateClient();
    private HttpResponseMessage? _response;
    private Guid _orderId;

    private HttpResponseMessage Response =>
        _response ?? throw new InvalidOperationException("No request has been sent yet.");

    [Given("a submitted order with lines")]
    public async Task GivenASubmittedOrderWithLines(DataTable lines)
    {
        var response = await _client.PostAsJsonAsync("/v1/orders", new SubmitOrderRequest(Lines(lines)));
        response.StatusCode.ShouldBe(HttpStatusCode.Created);
        _orderId = (await response.Content.ReadFromJsonAsync<OrderResponse>())!.Id;
    }

    [Given("the order has been confirmed")]
    public async Task GivenTheOrderHasBeenConfirmed()
    {
        var response = await _client.PostAsync($"/v1/orders/{_orderId}/confirm", null);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [Given("the order has been cancelled")]
    public async Task GivenTheOrderHasBeenCancelled()
    {
        var response = await _client.PostAsync($"/v1/orders/{_orderId}/cancel", null);
        response.StatusCode.ShouldBe(HttpStatusCode.NoContent);
    }

    [When("a client submits an order with lines")]
    public async Task WhenAClientSubmitsAnOrderWithLines(DataTable lines) =>
        _response = await _client.PostAsJsonAsync("/v1/orders", new SubmitOrderRequest(Lines(lines)));

    [When("a client submits an order with no lines")]
    public async Task WhenAClientSubmitsAnOrderWithNoLines() =>
        _response = await _client.PostAsJsonAsync("/v1/orders", new SubmitOrderRequest([]));

    [When("the client reads the order")]
    public async Task WhenTheClientReadsTheOrder() => _response = await _client.GetAsync($"/v1/orders/{_orderId}");

    [When("the client reads order {string}")]
    public async Task WhenTheClientReadsOrder(string orderId) =>
        _response = await _client.GetAsync($"/v1/orders/{orderId}");

    [When("the client confirms the order")]
    public async Task WhenTheClientConfirmsTheOrder() =>
        _response = await _client.PostAsync($"/v1/orders/{_orderId}/confirm", null);

    [When("the client cancels the order")]
    public async Task WhenTheClientCancelsTheOrder() =>
        _response = await _client.PostAsync($"/v1/orders/{_orderId}/cancel", null);

    [Then("the response status is {int}")]
    public void ThenTheResponseStatusIs(int status) => ((int)Response.StatusCode).ShouldBe(status);

    [Then("the response is an order with status {string} and total {decimal}")]
    public async Task ThenTheResponseIsAnOrderWithStatusAndTotal(string status, decimal total)
    {
        using var order = JsonDocument.Parse(await Response.Content.ReadAsStringAsync());
        order.RootElement.GetProperty("status").GetString().ShouldBe(status);
        order.RootElement.GetProperty("total").GetDecimal().ShouldBe(total);
    }

    [Then("the Location header points to the created order")]
    public async Task ThenTheLocationHeaderPointsToTheCreatedOrder()
    {
        var order = (await Response.Content.ReadFromJsonAsync<OrderResponse>())!;
        Response.Headers.Location.ShouldNotBeNull().ToString().ShouldEndWith($"/v1/orders/{order.Id}");
    }

    [Then("the response is a validation problem")]
    public async Task ThenTheResponseIsAValidationProblem()
    {
        ThenTheResponseIsAProblem();
        using var problem = JsonDocument.Parse(await Response.Content.ReadAsStringAsync());
        problem.RootElement.GetProperty("errors").EnumerateObject().ShouldNotBeEmpty();
    }

    [Then("the response is a problem")]
    public void ThenTheResponseIsAProblem() =>
        Response.Content.Headers.ContentType.ShouldNotBeNull().MediaType.ShouldBe("application/problem+json");

    [Then("reading the order shows status {string}")]
    public async Task ThenReadingTheOrderShowsStatus(string status)
    {
        using var order = JsonDocument.Parse(await _client.GetStringAsync($"/v1/orders/{_orderId}"));
        order.RootElement.GetProperty("status").GetString().ShouldBe(status);
    }

    private static List<OrderLineRequest> Lines(DataTable table) =>
        table
            .Rows.Select(row => new OrderLineRequest(
                row["sku"],
                int.Parse(row["quantity"], CultureInfo.InvariantCulture),
                decimal.Parse(row["unitPrice"], CultureInfo.InvariantCulture)
            ))
            .ToList();
}
