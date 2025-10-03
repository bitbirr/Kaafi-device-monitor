using System.ComponentModel.DataAnnotations;

namespace Kaafi.DeviceMonitor.Models;

public class Attendance
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int? DeviceId { get; set; }

    [Required]
    [StringLength(10)]
    public string InOut { get; set; } = "IN"; // IN or OUT

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public byte[]? Photo { get; set; }

    public Employee Employee { get; set; } = null!;
    public Device? Device { get; set; }
}