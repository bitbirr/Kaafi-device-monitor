using Microsoft.AspNetCore.Mvc;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
