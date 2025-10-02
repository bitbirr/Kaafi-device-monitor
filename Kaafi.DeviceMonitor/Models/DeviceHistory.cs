using System.ComponentModel.DataAnnotations;

namespace Kaafi.DeviceMonitor.Models;

public class DeviceHistory
{
    public int Id { get; set; }

    public int DeviceId { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = string.Empty;

    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public Device Device { get; set; } = null!;
}
