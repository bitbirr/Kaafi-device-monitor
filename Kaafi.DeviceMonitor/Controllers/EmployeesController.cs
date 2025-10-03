using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;

namespace Kaafi.DeviceMonitor.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public EmployeesController(ApplicationDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees(
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12)
    {
        var query = _context.Employees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => e.FullName.Contains(search) || e.Code.Contains(search) || e.Dept.Contains(search));
        }

        var totalRecords = await query.CountAsync();
        var employees = await query
            .OrderBy(e => e.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        Response.Headers.Add("X-Total-Count", totalRecords.ToString());

        return employees;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Employee>> GetEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }
        return employee;
    }

    [HttpPost]
    public async Task<ActionResult<Employee>> CreateEmployee(Employee employee)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(employee.Code))
        {
            return BadRequest("Employee code is required");
        }

        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            return BadRequest("Employee full name is required");
        }

        // Check for duplicate code
        var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Code == employee.Code);
        if (existingEmployee != null)
        {
            return BadRequest("Employee code already exists");
        }

        employee.CreatedAt = DateTime.UtcNow;
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateEmployee(int id, Employee employee)
    {
        if (id != employee.Id)
        {
            return BadRequest("ID mismatch");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validate required fields
        if (string.IsNullOrWhiteSpace(employee.Code))
        {
            return BadRequest("Employee code is required");
        }

        if (string.IsNullOrWhiteSpace(employee.FullName))
        {
            return BadRequest("Employee full name is required");
        }

        // Check for duplicate code (excluding current employee)
        var existingEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Code == employee.Code && e.Id != id);
        if (existingEmployee != null)
        {
            return BadRequest("Employee code already exists");
        }

        _context.Entry(employee).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!EmployeeExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteEmployee(int id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
        {
            return NotFound();
        }

        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("bulk")]
    public async Task<IActionResult> DeleteEmployees([FromBody] int[] ids)
    {
        var employees = await _context.Employees.Where(e => ids.Contains(e.Id)).ToListAsync();
        if (!employees.Any())
        {
            return NotFound();
        }

        _context.Employees.RemoveRange(employees);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool EmployeeExists(int id)
    {
        return _context.Employees.Any(e => e.Id == id);
    }
}