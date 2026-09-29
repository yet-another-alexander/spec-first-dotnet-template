using Microsoft.EntityFrameworkCore;

namespace SpecFirst.Service.Api.Persistence;

/// <summary>The persistence boundary. There is no repository layer: the seam is this context.</summary>
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Order> Orders => Set<Order>();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
}
