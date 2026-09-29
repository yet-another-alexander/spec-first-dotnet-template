using SpecFirst.Service.Models.Enums;
using Stateless;

namespace SpecFirst.Service.Api.Orders;

public sealed class Order
{
    public Guid Id { get; init; }
    public OrderStatus Status { get; private set; } = OrderStatus.Submitted;
    public DateTimeOffset CreatedAt { get; init; }
    public List<OrderLine> Lines { get; init; } = [];

    /// <summary>
    /// Applies a trigger to the lifecycle. False means the transition is not allowed from the current status; nothing
    /// changes then. The status is only ever changed here, so every transition is one line in the configuration.
    /// </summary>
    public bool TryFire(OrderTrigger trigger)
    {
        var lifecycle = Lifecycle();
        if (!lifecycle.CanFire(trigger))
            return false;

        lifecycle.Fire(trigger);
        return true;
    }

    private StateMachine<OrderStatus, OrderTrigger> Lifecycle()
    {
        var lifecycle = new StateMachine<OrderStatus, OrderTrigger>(() => Status, status => Status = status);

        // REQ-006: a submitted order is confirmed. REQ-007: confirming again changes nothing.
        lifecycle.Configure(OrderStatus.Submitted).Permit(OrderTrigger.Confirm, OrderStatus.Confirmed);
        lifecycle.Configure(OrderStatus.Confirmed).Ignore(OrderTrigger.Confirm);

        return lifecycle;
    }
}
