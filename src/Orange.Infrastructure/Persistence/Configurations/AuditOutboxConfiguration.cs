using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Common;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class AuditEntryConfiguration : IEntityTypeConfiguration<AuditEntry>
{
    public void Configure(EntityTypeBuilder<AuditEntry> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, v => new AuditEntryId(v));

        builder.Property(a => a.ActorId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));

        builder.Property(a => a.Action).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(a => a.EntityId).HasMaxLength(100);
        builder.Property(a => a.Details).HasMaxLength(2000);

        builder.HasIndex(a => a.Timestamp);
    }
}

public sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.EventType).HasMaxLength(200).IsRequired();
        builder.Property(m => m.Payload).HasColumnType("nvarchar(max)");
        builder.Property(m => m.LastError).HasMaxLength(2000);

        builder.HasIndex(m => m.ProcessedAt)
            .HasFilter("[ProcessedAt] IS NULL")
            .HasDatabaseName("IX_OutboxMessages_Unprocessed");
    }
}
