namespace Kaafi.DeviceMonitor.Models.DTOs;

public class StartEnrollRequest
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public int FingerIndex { get; set; }
}

public class StartEnrollResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? EmployeeId { get; set; }
}
