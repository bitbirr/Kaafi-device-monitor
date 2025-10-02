namespace Kaafi.DeviceMonitor.Models;

public class Employee
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    
    public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
}
