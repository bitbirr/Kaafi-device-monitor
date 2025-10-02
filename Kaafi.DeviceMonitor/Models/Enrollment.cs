using System.ComponentModel.DataAnnotations;

namespace Kaafi.DeviceMonitor.Models;

public class Enrollment
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }

    public int DeviceId { get; set; }

    public int FingerIndex { get; set; }

    [Required]
    public string Template { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Employee Employee { get; set; } = null!;
    public Device Device { get; set; } = null!;
}
