using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id)
            .HasConversion(id => id.Value, v => new LeaveRequestId(v));

        builder.Property(r => r.EmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));

        builder.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(r => r.Amount).HasPrecision(10, 2);
        builder.Property(r => r.Unit).HasConversion<string>().HasMaxLength(10);
        builder.Property(r => r.Reason).HasMaxLength(500);

        builder.Property(r => r.RowVersion).IsRowVersion();

        builder.OwnsOne(r => r.PartialTime, pt =>
        {
            pt.Property(t => t.From).HasColumnName("PartialFrom");
            pt.Property(t => t.To).HasColumnName("PartialTo");
        });

        builder.Property(r => r.Calculation)
            .HasConversion(
                c => JsonSerializer.Serialize(c, JsonOptions.Instance),
                json => JsonSerializer.Deserialize<LeaveCalculation>(json, JsonOptions.Instance)!)
            .HasColumnType("nvarchar(max)")
            .HasColumnName("CalculationSnapshot");

        builder.HasMany(r => r.Steps)
            .WithOne()
            .HasForeignKey("LeaveRequestId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(r => r.Status);
        builder.Ignore(r => r.DomainEvents);

        builder.HasIndex(r => r.EmployeeId);
        builder.HasIndex(r => new { r.StartDate, r.EndDate });
    }
}

public sealed class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, v => new ApprovalStepId(v));

        builder.Property(s => s.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.Rule).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.State).HasConversion<string>().HasMaxLength(20);

        builder.Property(s => s.ApproverIds)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids.Select(id => id.Value).ToList(), JsonOptions.Instance),
                json => JsonSerializer.Deserialize<List<Guid>>(json, JsonOptions.Instance)!
                    .Select(v => new EmployeeId(v)).ToList())
            .HasColumnType("nvarchar(max)");

        builder.OwnsMany(s => s.Decisions, d =>
        {
            d.WithOwner().HasForeignKey("ApprovalStepId");
            d.Property<int>("Id").ValueGeneratedOnAdd();
            d.HasKey("Id");
            d.Property(x => x.DecidedBy)
                .HasConversion(id => id.Value, v => new EmployeeId(v));
            d.Property(x => x.Decision).HasConversion<string>().HasMaxLength(20);
            d.Property(x => x.Comment).HasMaxLength(500);
            d.Property(x => x.OnBehalfOf)
                .HasConversion(
                    id => id.HasValue ? id.Value.Value : (Guid?)null,
                    v => v.HasValue ? new EmployeeId(v.Value) : null);
        });

    }
}

internal static class JsonOptions
{
    public static readonly JsonSerializerOptions Instance = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };
}
