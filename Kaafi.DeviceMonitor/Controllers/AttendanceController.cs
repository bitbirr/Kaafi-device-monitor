// Adding missing usings
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class AttendanceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<AttendanceController> _logger;

    public AttendanceController(ApplicationDbContext context, ILogger<AttendanceController> logger)
    {
        _context = context;
        _logger = logger;
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
            if (int.TryParse(deviceFilter, out var deviceId))
            {
                query = query.Where(a => a.DeviceId == deviceId);
            }
            else
            {
                return BadRequest("Invalid device filter. Must be a valid device ID.");
            }
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
        _logger.LogInformation("Creating attendance record for EmployeeId: {EmployeeId}, InOut: {InOut}", attendance.EmployeeId, attendance.InOut);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Invalid model state for attendance creation");
            return BadRequest(ModelState);
        }

        // Validate required fields
        if (attendance.EmployeeId <= 0)
        {
            _logger.LogWarning("Invalid EmployeeId: {EmployeeId}", attendance.EmployeeId);
            return BadRequest("Valid EmployeeId is required");
        }

        if (string.IsNullOrWhiteSpace(attendance.InOut) || (attendance.InOut != "IN" && attendance.InOut != "OUT"))
        {
            _logger.LogWarning("Invalid InOut value: {InOut}", attendance.InOut);
            return BadRequest("InOut must be either 'IN' or 'OUT'");
        }

        // Check if employee exists
        var employeeExists = await _context.Employees.AnyAsync(e => e.Id == attendance.EmployeeId);
        if (!employeeExists)
        {
            _logger.LogWarning("Employee not found: {EmployeeId}", attendance.EmployeeId);
            return BadRequest("Employee not found");
        }

        // Check if device exists (if provided)
        if (attendance.DeviceId.HasValue)
        {
            var deviceExists = await _context.Devices.AnyAsync(d => d.Id == attendance.DeviceId.Value);
            if (!deviceExists)
            {
                _logger.LogWarning("Device not found: {DeviceId}", attendance.DeviceId.Value);
                return BadRequest("Device not found");
            }
        }

        try
        {
            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Attendance record created successfully with Id: {Id}", attendance.Id);
            return CreatedAtAction(nameof(GetAttendance), new { id = attendance.Id }, attendance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating attendance record");
            throw;
        }
    }

    [HttpGet("devices")]
    public async Task<ActionResult<IEnumerable<Device>>> GetDevices()
    {
        return await _context.Devices.OrderBy(d => d.Name).ToListAsync();
    }
}