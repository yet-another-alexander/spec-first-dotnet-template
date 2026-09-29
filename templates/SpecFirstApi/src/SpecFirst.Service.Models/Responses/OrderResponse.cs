using System.ComponentModel;
using SpecFirst.Service.Models.Enums;

namespace SpecFirst.Service.Models.Responses;

public sealed record OrderResponse(
    [property: Description("Order identifier.")] Guid Id,
    [property: Description("Lifecycle status of the order.")] OrderStatus Status,
    [property: Description("Sum of quantity multiplied by unit price over all lines.")] decimal Total,
    [property: Description("Lines of the order in submission order.")] IReadOnlyList<OrderLineResponse> Lines,
    [property: Description("When the order was submitted, UTC.")] DateTimeOffset CreatedAt
);
