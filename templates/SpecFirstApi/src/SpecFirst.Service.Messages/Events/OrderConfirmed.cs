using System.ComponentModel;
using SpecFirst.Service.Messages;

namespace SpecFirst.Service.Messages.Events;

[MessageTopic(Topic)]
public sealed record OrderConfirmed(
    [property: Description("Identifier of the confirmed order.")] Guid OrderId,
    [property: Description("When the order was confirmed, UTC.")] DateTimeOffset ConfirmedAt
)
{
    public const string Topic = "orders.confirmed";
}
