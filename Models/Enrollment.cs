namespace Kaafi.DeviceMonitor.Models;

public class Enrollment
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public int EnrollId { get; set; }
    public int FingerIndex { get; set; }
    public string Template { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    
    public Employee? Employee { get; set; }
}
