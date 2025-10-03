using System.ComponentModel.DataAnnotations;

namespace Kaafi.DeviceMonitor.Models;

public class Device
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string IP { get; set; } = string.Empty;

    public int Port { get; set; } = 4370;

    [StringLength(20)]
    public string Status { get; set; } = "Unknown";

    public DateTime? LastActive { get; set; }

    public ICollection<DeviceHistory> History { get; set; } = new List<DeviceHistory>();
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
