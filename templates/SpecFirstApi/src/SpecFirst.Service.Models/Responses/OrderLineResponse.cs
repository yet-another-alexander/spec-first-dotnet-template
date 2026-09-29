using System.ComponentModel;

namespace SpecFirst.Service.Models.Responses;

public sealed record OrderLineResponse(
    [property: Description("Stock keeping unit of the product.")] string Sku,
    [property: Description("Number of units.")] int Quantity,
    [property: Description("Price of one unit.")] decimal UnitPrice
);
