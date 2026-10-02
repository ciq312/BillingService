using Microsoft.EntityFrameworkCore;
using PaymentService.Domain;

namespace PaymentService.Infrastructure;

public class AppDbContext : DbContext
{
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Payment>(b =>
        {
            b.HasKey(p => p.Id);
            b.HasIndex(p => p.OrderId).IsUnique();
        });

        modelBuilder.Entity<InboxMessage>(b =>
        {
            b.ToTable("InboxMessages");
            b.HasKey(im => new { im.MessageId, im.Consumer });
            b.HasIndex(im => im.ProcessedAt);
        });
    }
}
