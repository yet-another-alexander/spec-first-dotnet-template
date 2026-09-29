namespace SpecFirst.Service.Api.Orders;

/// <summary>REQ-001: the total is the sum of quantity multiplied by unit price over the lines. Pure, so property-tested.</summary>
public static class OrderTotal
{
    public static decimal Of(IEnumerable<OrderLine> lines) => lines.Sum(line => line.Quantity * line.UnitPrice);
}
