using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AttendanceController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Attendance>>> GetAttendance(
        [FromQuery] string? employeeFilter = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? deviceFilter = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var query = _context.Attendances
            .Include(a => a.Employee)
            .Include(a => a.Device)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(employeeFilter))
        {
            query = query.Where(a => a.Employee != null && (a.Employee.FullName.Contains(employeeFilter) || a.Employee.Code.Contains(employeeFilter)));
        }

        if (dateFrom.HasValue)
        {
            query = query.Where(a => a.Timestamp >= dateFrom.Value.Date);
        }

        if (dateTo.HasValue)
        {
            query = query.Where(a => a.Timestamp <= dateTo.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(deviceFilter))
        {
            var deviceId = int.Parse(deviceFilter);
            query = query.Where(a => a.DeviceId == deviceId);
        }

        var totalRecords = await query.CountAsync();
        var attendance = await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalRecords.ToString());

        return attendance;
    }

    [HttpPost]
    public async Task<ActionResult<Attendance>> CreateAttendance(Attendance attendance)
    {
        _context.Attendances.Add(attendance);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, attendance);
    }

    [HttpGet("devices")]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        return await _context.Devices.OrderBy(d => d.Name).ToListAsync();
    }
}