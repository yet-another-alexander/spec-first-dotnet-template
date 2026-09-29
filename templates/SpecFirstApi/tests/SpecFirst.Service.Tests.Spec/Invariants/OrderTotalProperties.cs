using CsCheck;

namespace SpecFirst.Service.Tests.Spec.Invariants;

/// <summary>
/// REQ-001 is ubiquitous: an invariant over any set of lines. It is proven over random inputs, not hand-picked examples.
/// </summary>
[Collection(InvariantsCollection.Name)]
public sealed class OrderTotalProperties
{
    // Prices as whole cents: exact decimals, no filtering of a huge range.
    private static readonly Gen<OrderLine> Line = Gen.Select(
        Gen.Int[1, 1_000],
        Gen.Int[0, 1_000_000],
        (quantity, cents) =>
            new OrderLine
            {
                Sku = "SKU",
                Quantity = quantity,
                UnitPrice = cents / 100m,
            }
    );

    private static readonly Gen<List<OrderLine>> Lines = Line.List[0, 20];

    [Fact]
    [Requirement("REQ-001", "7f6cdb52")]
    public void The_total_of_no_lines_is_zero() => OrderTotal.Of([]).ShouldBe(0m);

    [Fact]
    [Requirement("REQ-001", "7f6cdb52")]
    public void Each_line_contributes_quantity_times_unit_price() =>
        Line.Sample(line => OrderTotal.Of([line]).ShouldBe(line.Quantity * line.UnitPrice));

    [Fact]
    [Requirement("REQ-001", "7f6cdb52")]
    public void The_total_is_additive_over_line_sets() =>
        Gen.Select(Lines, Lines)
            .Sample(
                (first, second) =>
                    OrderTotal.Of(first.Concat(second)).ShouldBe(OrderTotal.Of(first) + OrderTotal.Of(second))
            );

    [Fact]
    [Requirement("REQ-001", "7f6cdb52")]
    public void The_total_does_not_depend_on_line_order() =>
        Lines.Sample(lines => OrderTotal.Of(lines).ShouldBe(OrderTotal.Of(lines.AsEnumerable().Reverse())));
}
