using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SpecFirst.Service.Models.Requests;

public sealed record SubmitOrderRequest(
    [property: Description("Lines of the order; at least one."), MinLength(1)] IReadOnlyList<OrderLineRequest> Lines
);
