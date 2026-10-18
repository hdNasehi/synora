using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Common;
using Orange.Domain.Employees;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, v => new EmployeeId(v));

        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Email).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Fte).HasPrecision(5, 2);

        builder.Property(e => e.TeamId)
            .HasConversion(id => id.Value, v => new TeamId(v));
        builder.Property(e => e.DepartmentId)
            .HasConversion(id => id.Value, v => new DepartmentId(v));
        builder.Property(e => e.HolidayGroupId)
            .HasConversion(id => id.Value, v => new HolidayGroupId(v));

        builder.HasIndex(e => e.Email).IsUnique();
    }
}

public sealed class TeamConfiguration : IEntityTypeConfiguration<Team>
{
    public void Configure(EntityTypeBuilder<Team> builder)
    {
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id)
            .HasConversion(id => id.Value, v => new TeamId(v));

        builder.Property(t => t.Name).HasMaxLength(100).IsRequired();
        builder.Property(t => t.DepartmentId)
            .HasConversion(id => id.Value, v => new DepartmentId(v));
    }
}

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id)
            .HasConversion(id => id.Value, v => new DepartmentId(v));

        builder.Property(d => d.Name).HasMaxLength(100).IsRequired();
    }
}
