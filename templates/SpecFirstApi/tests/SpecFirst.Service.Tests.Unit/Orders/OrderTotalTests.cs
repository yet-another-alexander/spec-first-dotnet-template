using SpecFirst.Service.Api.Orders;

namespace SpecFirst.Service.Tests.Unit.Orders;

public sealed class OrderTotalTests
{
    [Fact]
    public void Two_lines_add_up()
    {
        OrderLine[] lines =
        [
            new()
            {
                Sku = "APPLE",
                Quantity = 3,
                UnitPrice = 1.50m,
            },
            new()
            {
                Sku = "PEAR",
                Quantity = 1,
                UnitPrice = 2.25m,
            },
        ];

        OrderTotal.Of(lines).ShouldBe(6.75m);
    }
}
