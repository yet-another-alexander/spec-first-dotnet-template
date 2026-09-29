namespace SpecFirst.Service.Api.Orders;

/// <summary>What a client can ask of an order. The transitions each trigger permits are configured in <see cref="Order"/>.</summary>
public enum OrderTrigger
{
    Confirm,
}
