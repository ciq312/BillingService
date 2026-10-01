using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Models;
using OrderService.DAL.Models.Orders;

namespace OrderService.DAL.Db;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<CustomOutboxMessage> OutboxMessages => Set<CustomOutboxMessage>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().
            HasKey(o => o.Id);

        modelBuilder.Entity<CustomOutboxMessage>().
            HasKey(m => m.MessageId);

    }
}