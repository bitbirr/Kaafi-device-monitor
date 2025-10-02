using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kaafi.DeviceMonitor.Models.DTOs;
using Kaafi.DeviceMonitor.Services;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class DeviceController : ControllerBase
{
    private readonly IZKTecoService _zkService;
    private readonly ILogger<DeviceController> _logger;

    public DeviceController(IZKTecoService zkService, ILogger<DeviceController> logger)
    {
        _zkService = zkService;
        _logger = logger;
    }

    [HttpPost("connect")]
    public IActionResult Connect([FromBody] ConnectRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.IpAddress))
            {
                return BadRequest(new ConnectResponse 
                { 
                    Success = false, 
                    Message = "IP address is required" 
                });
            }

            var connected = _zkService.Connect(request.IpAddress, request.Port);
            
            if (connected)
            {
                var deviceId = _zkService.GetDeviceSerialNumber();
                _logger.LogInformation("Device connected: {DeviceId}", deviceId);
                
                return Ok(new ConnectResponse
                {
                    Success = true,
                    Message = "Connected successfully",
                    DeviceId = deviceId
                });
            }
            else
            {
                return BadRequest(new ConnectResponse
                {
                    Success = false,
                    Message = "Failed to connect to device"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error connecting to device");
            return StatusCode(500, new ConnectResponse
            {
                Success = false,
                Message = $"Internal error: {ex.Message}"
            });
        }
    }

    [HttpPost("disconnect")]
    public IActionResult Disconnect()
    {
        try
        {
            _zkService.Disconnect();
            return Ok(new { Success = true, Message = "Disconnected successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting from device");
            return StatusCode(500, new { Success = false, Message = $"Internal error: {ex.Message}" });
        }
    }

    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        var isConnected = _zkService.IsConnected();
        var deviceId = isConnected ? _zkService.GetDeviceSerialNumber() : null;
        
        return Ok(new 
        { 
            IsConnected = isConnected,
            DeviceId = deviceId
        });
    }
}
