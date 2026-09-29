namespace SpecFirst.Service.Api.Orders;

public sealed class OrderLine
{
    public long Id { get; init; }
    public Guid OrderId { get; init; }
    public required string Sku { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}
