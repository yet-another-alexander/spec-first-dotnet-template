namespace SpecFirst.Service.Tests.Unit.Orders;

public sealed class OrderLifecycleTests
{
    [Fact]
    public void A_new_order_is_submitted() => new Order().Status.ShouldBe(OrderStatus.Submitted);

    [Fact]
    public void Confirm_moves_a_submitted_order_to_confirmed()
    {
        var order = new Order();

        order.TryFire(OrderTrigger.Confirm).ShouldBeTrue();

        order.Status.ShouldBe(OrderStatus.Confirmed);
    }

    [Fact]
    public void Confirm_on_a_confirmed_order_is_accepted_and_changes_nothing()
    {
        var order = new Order();
        order.TryFire(OrderTrigger.Confirm);

        order.TryFire(OrderTrigger.Confirm).ShouldBeTrue();

        order.Status.ShouldBe(OrderStatus.Confirmed);
    }
}
