using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Balances;
using Orange.Domain.Common;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class LeavePolicyConfiguration : IEntityTypeConfiguration<LeavePolicy>
{
    public void Configure(EntityTypeBuilder<LeavePolicy> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, v => new LeavePolicyId(v));

        builder.Property(p => p.Name).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Unit).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Entitlement).HasPrecision(10, 2);
        builder.Property(p => p.CarryOverMax).HasPrecision(10, 2);
    }
}

public sealed class BalanceAdjustmentConfiguration : IEntityTypeConfiguration<BalanceAdjustment>
{
    public void Configure(EntityTypeBuilder<BalanceAdjustment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Amount).HasPrecision(10, 2);
        builder.Property(a => a.Reason).HasMaxLength(500);

        builder.HasIndex(a => new { a.EmployeeId, a.Year });
    }
}
