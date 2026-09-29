using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SpecFirst.Service.Api.Orders;

public sealed class OrderLineConfiguration : IEntityTypeConfiguration<OrderLine>
{
    public void Configure(EntityTypeBuilder<OrderLine> line)
    {
        line.ToTable("order_lines");
        line.HasKey(l => l.Id);
        line.Property(l => l.Sku).HasMaxLength(64);
        line.Property(l => l.UnitPrice).HasPrecision(18, 2);
    }
}
