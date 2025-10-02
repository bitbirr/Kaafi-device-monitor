using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices { get; set; }
    public DbSet<DeviceHistory> DeviceHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.IP).IsRequired();
            entity.Property(e => e.Port).HasDefaultValue(4370);
            entity.HasMany(e => e.History)
                .WithOne(e => e.Device)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DeviceHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Status).IsRequired();
            entity.Property(e => e.Timestamp).IsRequired();
        });
    }
}
