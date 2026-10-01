using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.DAL.Models.Outbox;

namespace OrderService.DAL.Db.Configuration;

public class CustomOutboxMessageConfiguration : IEntityTypeConfiguration<CustomOutboxMessage>
{
    public void Configure(EntityTypeBuilder<CustomOutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages", t => t.HasCheckConstraint(
            "CK_Outbox_Status",
            "\"Status\" IN ('Pending', 'Processed', 'Parked')"));

        builder.HasKey(m => m.MessageId);

        builder.Property(m => m.Status)
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(m => m.CreatedAt)
            .HasFilter("\"Status\" = 'Pending'")
            .HasDatabaseName("IX_OutboxMessages_Pending");
    }
}