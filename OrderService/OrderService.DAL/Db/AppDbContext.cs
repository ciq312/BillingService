using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Models.Orders;
using OrderService.DAL.Models.Outbox;

namespace OrderService.DAL.Db;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CustomOutboxMessage> OutboxMessages => Set<CustomOutboxMessage>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}