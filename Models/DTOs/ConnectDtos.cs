namespace Kaafi.DeviceMonitor.Models.DTOs;

public class ConnectRequest
{
    public string IpAddress { get; set; } = string.Empty;
    public int Port { get; set; } = 4370;
}

public class ConnectResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? DeviceId { get; set; }
}
