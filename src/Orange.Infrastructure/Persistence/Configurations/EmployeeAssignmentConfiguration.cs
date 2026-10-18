using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Common;
using Orange.Domain.Employees;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class EmployeeAssignmentConfiguration : IEntityTypeConfiguration<EmployeeAssignment>
{
    public void Configure(EntityTypeBuilder<EmployeeAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.ManagerId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.Rule).HasConversion<string>().HasMaxLength(20);

        builder.Property(a => a.SupervisorIds)
            .HasConversion(
                ids => JsonSerializer.Serialize(ids.Select(id => id.Value).ToList(), JsonSerializerOptions.Default),
                json => JsonSerializer.Deserialize<List<Guid>>(json, JsonSerializerOptions.Default)!
                    .Select(v => new EmployeeId(v)).ToList())
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(a => new { a.EmployeeId, a.ValidFrom });
    }
}

public sealed class ScheduleAssignmentConfiguration : IEntityTypeConfiguration<ScheduleAssignment>
{
    public void Configure(EntityTypeBuilder<ScheduleAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.ScheduleId)
            .HasConversion(id => id.Value, v => new WorkScheduleId(v));

        builder.HasIndex(a => new { a.EmployeeId, a.ValidFrom });
    }
}

public sealed class PolicyAssignmentConfiguration : IEntityTypeConfiguration<PolicyAssignment>
{
    public void Configure(EntityTypeBuilder<PolicyAssignment> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.EmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(a => a.PolicyId)
            .HasConversion(id => id.Value, v => new LeavePolicyId(v));

        builder.HasIndex(a => new { a.EmployeeId, a.ValidFrom });
    }
}

public sealed class SubstituteConfiguration : IEntityTypeConfiguration<Substitute>
{
    public void Configure(EntityTypeBuilder<Substitute> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.ForEmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
        builder.Property(s => s.SubstituteEmployeeId)
            .HasConversion(id => id.Value, v => new EmployeeId(v));
    }
}
