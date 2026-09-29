using System.ComponentModel;
using DotNetCore.CAP;
using Microsoft.EntityFrameworkCore;

namespace SpecFirst.Service.Api.Orders;

public static class OrderEndpoints
{
    public static void MapOrders(this IEndpointRouteBuilder app)
    {
        var orders = app.MapGroup("/v1/orders").WithTags("Orders");

        orders
            .MapPost("", SubmitOrder)
            .WithName("submitOrder")
            .WithSummary("Submit a new order")
            .WithDescription("Stores the order with status submitted and returns its representation.")
            .Produces<OrderResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        orders
            .MapGet("/{orderId:guid}", GetOrder)
            .WithName("getOrder")
            .WithSummary("Read an order")
            .WithDescription("Returns the order with its lines, status and total.")
            .Produces<OrderResponse>()
            .ProducesProblem(StatusCodes.Status404NotFound);

        orders
            .MapPost("/{orderId:guid}/confirm", ConfirmOrder)
            .WithName("confirmOrder")
            .WithSummary("Confirm a submitted order")
            .WithDescription(
                "Moves a submitted order to confirmed. Confirming an order that is already confirmed changes nothing."
            )
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> SubmitOrder(
        SubmitOrderRequest request,
        AppDbContext db,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        var order = new Order
        {
            Id = Guid.CreateVersion7(),
            CreatedAt = clock.GetUtcNow(),
            Lines = request
                .Lines.Select(line => new OrderLine
                {
                    Sku = line.Sku,
                    Quantity = line.Quantity,
                    UnitPrice = line.UnitPrice,
                })
                .ToList(),
        };
        db.Orders.Add(order);
        await db.SaveChangesAsync(cancellationToken);

        return TypedResults.CreatedAtRoute(ToResponse(order), "getOrder", new { orderId = order.Id });
    }

    private static async Task<IResult> GetOrder(
        [Description("Order identifier.")] Guid orderId,
        AppDbContext db,
        CancellationToken cancellationToken
    )
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        return order is null ? OrderNotFound() : TypedResults.Ok(ToResponse(order));
    }

    private static async Task<IResult> ConfirmOrder(
        [Description("Order identifier.")] Guid orderId,
        AppDbContext db,
        ICapPublisher publisher,
        TimeProvider clock,
        CancellationToken cancellationToken
    )
    {
        var order = await db.Orders.SingleOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
            return OrderNotFound();

        var wasSubmitted = order.Status == OrderStatus.Submitted;
        if (!order.TryFire(OrderTrigger.Confirm))
            return TransitionNotAllowed();

        if (!wasSubmitted)
            return TypedResults.NoContent(); // REQ-007, TECH-006: nothing changed, nothing published.

        try
        {
            // TECH-005: the status change and the outbox row commit together; CAP relays the message afterwards.
            await using var transaction = await db.Database.BeginTransactionAsync(
                publisher,
                autoCommit: false,
                cancellationToken
            );
            await db.SaveChangesAsync(cancellationToken);
            await publisher.PublishAsync(
                OrderConfirmed.Topic,
                new OrderConfirmed(order.Id, clock.GetUtcNow()),
                cancellationToken: cancellationToken
            );
            await transaction.CommitAsync(cancellationToken);
            return TypedResults.NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request changed the order between our read and our write (a client retry landing on a second
            // instance, say). Its change and its outbox row are committed; ours rolled back with the transaction.
            // Answer from the current state as if this request had arrived after it: confirmed is the idempotent 204
            // (REQ-007, TECH-006), anything else is the rejected transition.
            await db.Entry(order).ReloadAsync(cancellationToken);
            return order.Status == OrderStatus.Confirmed ? TypedResults.NoContent() : TransitionNotAllowed();
        }
    }

    private static IResult OrderNotFound() =>
        TypedResults.Problem(statusCode: StatusCodes.Status404NotFound, title: "Order not found.");

    private static IResult TransitionNotAllowed() =>
        TypedResults.Problem(
            statusCode: StatusCodes.Status409Conflict,
            title: "The order's status does not allow this."
        );

    private static OrderResponse ToResponse(Order order) =>
        new(
            order.Id,
            order.Status,
            OrderTotal.Of(order.Lines),
            order.Lines.Select(line => new OrderLineResponse(line.Sku, line.Quantity, line.UnitPrice)).ToList(),
            order.CreatedAt
        );
}
