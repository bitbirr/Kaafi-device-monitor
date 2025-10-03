using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EnrollmentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EnrollmentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Enrollment>>> GetEnrollments()
    {
        return await _context.Enrollments
            .Include(e => e.Employee)
            .Include(e => e.Device)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    [HttpPost]
    public async Task<ActionResult<Enrollment>> CreateEnrollment(Enrollment enrollment)
    {
        _context.Enrollments.Add(enrollment);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEnrollments), new { id = enrollment.Id }, enrollment);
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateEnrollments([FromBody] Enrollment[] enrollments)
    {
        _context.Enrollments.AddRange(enrollments);
        await _context.SaveChangesAsync();
        return Ok();
    }
}