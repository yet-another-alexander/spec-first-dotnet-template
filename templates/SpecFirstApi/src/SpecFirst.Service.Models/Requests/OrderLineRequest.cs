using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpecFirst.Service.Models.Requests;

public sealed record OrderLineRequest(
    [property: Description("Stock keeping unit of the product."), Required, MaxLength(64)] string Sku,
    [property: Description("Number of units; at least 1."), Range(1, int.MaxValue)] int Quantity,
    [property: Description("Price of one unit; zero or more."), Range(0, double.MaxValue)] decimal UnitPrice
);
