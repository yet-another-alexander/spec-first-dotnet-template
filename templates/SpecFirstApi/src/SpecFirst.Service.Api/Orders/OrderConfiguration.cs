using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpecFirst.Service.Api.Orders;

public sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> order)
    {
        order.HasKey(o => o.Id);
        // Optimistic concurrency on Postgres's own xmin system column (Npgsql's replacement for the removed
        // UseXminAsConcurrencyToken): two requests that read the same row version race on the write, and the loser gets
        // DbUpdateConcurrencyException instead of silently repeating the transition (TECH-006). No column is added.
        order.Property<uint>("xmin").IsRowVersion();
        order.Property(o => o.Status).HasConversion<string>().HasMaxLength(16);
        order.HasMany(o => o.Lines).WithOne().HasForeignKey(line => line.OrderId).OnDelete(DeleteBehavior.Cascade);
        order.Navigation(o => o.Lines).AutoInclude();
    }
}
