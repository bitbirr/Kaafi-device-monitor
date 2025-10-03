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
    public DbSet<Employee> Employees { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }

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

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Code).IsRequired();
            entity.Property(e => e.FullName).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasMany(e => e.Enrollments)
                .WithOne(e => e.Employee)
                .HasForeignKey(e => e.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Enrollment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.EnrollId).IsRequired();
            entity.Property(e => e.Template).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasOne(e => e.Device)
                .WithMany(d => d.Enrollments)
                .HasForeignKey(e => e.DeviceId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
