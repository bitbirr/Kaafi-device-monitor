using System.ComponentModel.DataAnnotations;

namespace Kaafi.DeviceMonitor.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int DeviceId { get; set; }

    [Required]
    [StringLength(50)]
    public string EnrollId { get; set; } = string.Empty;

    public int FingerIndex { get; set; }

    [Required]
    public byte[] Template { get; set; } = Array.Empty<byte>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public Device Device { get; set; } = null!;
}
