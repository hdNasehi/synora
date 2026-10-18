using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Orange.Domain.Calendars;
using Orange.Domain.Common;

namespace Orange.Infrastructure.Persistence.Configurations;

public sealed class WorkScheduleConfiguration : IEntityTypeConfiguration<WorkSchedule>
{
    public void Configure(EntityTypeBuilder<WorkSchedule> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .HasConversion(id => id.Value, v => new WorkScheduleId(v));

        builder.Property(s => s.Code).HasMaxLength(10).IsRequired();
        builder.Property(s => s.Name).HasMaxLength(200).IsRequired();

        builder.OwnsMany(s => s.Days, d =>
        {
            d.WithOwner().HasForeignKey("WorkScheduleId");
            d.Property<int>("Id").ValueGeneratedOnAdd();
            d.HasKey("Id");
            d.Property(w => w.DayOfWeek).HasConversion<int>();
            d.Property(w => w.Hours).HasPrecision(5, 2);
        });

        builder.HasIndex(s => s.Code).IsUnique();
    }
}

public sealed class HolidayConfiguration : IEntityTypeConfiguration<Holiday>
{
    public void Configure(EntityTypeBuilder<Holiday> builder)
    {
        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id)
            .HasConversion(id => id.Value, v => new HolidayId(v));

        builder.Property(h => h.Name).HasMaxLength(200).IsRequired();
        builder.Property(h => h.Kind).HasMaxLength(20).IsRequired();

        builder.HasIndex(h => h.Date);
    }
}

public sealed class HolidayGroupConfiguration : IEntityTypeConfiguration<HolidayGroup>
{
    public void Configure(EntityTypeBuilder<HolidayGroup> builder)
    {
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id)
            .HasConversion(id => id.Value, v => new HolidayGroupId(v));

        builder.Property(g => g.Name).HasMaxLength(100).IsRequired();
    }
}

public sealed class HolidayObservanceConfiguration : IEntityTypeConfiguration<HolidayObservance>
{
    public void Configure(EntityTypeBuilder<HolidayObservance> builder)
    {
        builder.HasKey(o => new { o.HolidayId, o.GroupId });

        builder.Property(o => o.HolidayId)
            .HasConversion(id => id.Value, v => new HolidayId(v));
        builder.Property(o => o.GroupId)
            .HasConversion(id => id.Value, v => new HolidayGroupId(v));
    }
}
