using System.Collections.Concurrent;
using System.Diagnostics;
using DotNetCore.CAP;

namespace SpecFirst.Service.Tests.Spec.Integration;

/// <summary>
/// A CAP subscriber the test host registers so the tests see what the service publishes, as any other consumer on the
/// broker would. Messages are kept per topic; tests filter by their own order id because the host is shared.
/// </summary>
public sealed class MessageSink : ICapSubscribe
{
    private readonly ConcurrentQueue<OrderConfirmed> _orderConfirmed = new();

    public IReadOnlyCollection<OrderConfirmed> OrderConfirmed => _orderConfirmed;

    [CapSubscribe(Messages.Events.OrderConfirmed.Topic, Group = "specfirst-service.tests")]
    public void OnOrderConfirmed(OrderConfirmed message) => _orderConfirmed.Enqueue(message);

    public async Task<IReadOnlyList<OrderConfirmed>> OrderConfirmedFor(Guid orderId, TimeSpan settle)
    {
        var waited = Stopwatch.StartNew();
        while (waited.Elapsed < settle && !_orderConfirmed.Any(m => m.OrderId == orderId))
            await Task.Delay(100);

        // The message may not be the last to arrive; give a duplicate a moment to show up.
        await Task.Delay(500);
        return _orderConfirmed.Where(m => m.OrderId == orderId).ToList();
    }
}
