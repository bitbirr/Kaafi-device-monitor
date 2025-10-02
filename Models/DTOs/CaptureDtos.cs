namespace Kaafi.DeviceMonitor.Models.DTOs;

public class CaptureRequest
{
    public int EmployeeId { get; set; }
    public int FingerIndex { get; set; }
    public int CaptureNumber { get; set; }
}

public class CaptureResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int CaptureCount { get; set; }
    public bool IsComplete { get; set; }
    public string? Template { get; set; }
}

public class SaveEnrollRequest
{
    public int EmployeeId { get; set; }
    public int FingerIndex { get; set; }
    public string Template { get; set; } = string.Empty;
}

public class SaveEnrollResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public int? EnrollmentId { get; set; }
}
