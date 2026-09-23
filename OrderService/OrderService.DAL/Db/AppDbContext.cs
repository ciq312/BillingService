using Microsoft.EntityFrameworkCore;
using OrderService.DAL.Models.Orders;

namespace OrderService.DAL.Db;

public class AppDbContext : DbContext
{
    public DbSet<Order> Orders { get; private set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>().
            HasKey(o => o.Id).
            HasName("Id_PrimaryKey");

    }
}