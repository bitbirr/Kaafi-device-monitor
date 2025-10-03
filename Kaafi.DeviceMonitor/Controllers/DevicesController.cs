using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;
using Kaafi.DeviceMonitor.Services;

namespace Kaafi.DeviceMonitor.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IServiceProvider _serviceProvider;

    public DevicesController(ApplicationDbContext context, IServiceProvider serviceProvider)
    {
        _context = context;
        _serviceProvider = serviceProvider;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices([FromQuery] string? search = null)
    {
        var query = _context.Devices.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(d => d.Name.Contains(search) || d.IP.Contains(search));
        }

        return await query.OrderBy(d => d.Id).ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Device>> GetDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }
        return device;
    }

    [HttpPost]
    public async Task<ActionResult<Device>> CreateDevice(Device device)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(device.Name))
        {
            return BadRequest("Device name is required");
        }

        if (string.IsNullOrWhiteSpace(device.IP))
        {
            return BadRequest("Device IP address is required");
        }

        // Basic IP address validation
        if (!System.Net.IPAddress.TryParse(device.IP, out _))
        {
            return BadRequest("Invalid IP address format");
        }

        // Check for duplicate IP
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.IP == device.IP);
        if (existingDevice != null)
        {
            return BadRequest("Device with this IP address already exists");
        }

        device.Status = "Unknown";
        _context.Devices.Add(device);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetDevice), new { id = device.Id }, device);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateDevice(int id, Device device)
    {
        if (id != device.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(device.Name))
        {
            return BadRequest("Device name is required");
        }

        if (string.IsNullOrWhiteSpace(device.IP))
        {
            return BadRequest("Device IP address is required");
        }

        // Basic IP address validation
        if (!System.Net.IPAddress.TryParse(device.IP, out _))
        {
            return BadRequest("Invalid IP address format");
        }

        // Check for duplicate IP (excluding current device)
        var existingDevice = await _context.Devices.FirstOrDefaultAsync(d => d.IP == device.IP && d.Id != id);
        if (existingDevice != null)
        {
            return BadRequest("Device with this IP address already exists");
        }

        _context.Entry(device).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!DeviceExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        _context.Devices.Remove(device);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{id}/ping")]
    public async Task<ActionResult<string>> PingDevice(int id)
    {
        var device = await _context.Devices.FindAsync(id);
        if (device == null)
        {
            return NotFound();
        }

        var status = await DevicePingService.PingDeviceAsync(device.IP);
        device.Status = status;

        if (status == "Online")
        {
            device.LastActive = DateTime.UtcNow;
        }

        // Log to history
        var history = new DeviceHistory
        {
            DeviceId = device.Id,
            Status = status,
            Timestamp = DateTime.UtcNow
        };

        _context.DeviceHistories.Add(history);
        await _context.SaveChangesAsync();

        return status;
    }

    [HttpGet("{id}/history")]
    public async Task<ActionResult<IEnumerable<DeviceHistory>>> GetDeviceHistory(
        int id,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.DeviceHistories
            .Where(h => h.DeviceId == id);

        if (startDate.HasValue)
        {
            query = query.Where(h => h.Timestamp >= startDate.Value.Date);
        }

        if (endDate.HasValue)
        {
            query = query.Where(h => h.Timestamp <= endDate.Value.Date.AddDays(1));
        }

        var totalRecords = await query.CountAsync();
        var history = await query
            .OrderByDescending(h => h.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalRecords.ToString());

        return history;
    }

    private bool DeviceExists(int id)
    {
        return _context.Devices.Any(e => e.Id == id);
    }
}