using Kaafi.DeviceMonitor.Data;
using Kaafi.DeviceMonitor.Models;
using Microsoft.EntityFrameworkCore;

namespace Kaafi.DeviceMonitor.Services;

public interface IEnrollmentService
{
    Task<Employee?> GetOrCreateEmployeeAsync(string code, string fullName);
    Task<Employee?> GetEmployeeByIdAsync(int id);
    Task<Enrollment?> SaveEnrollmentAsync(int employeeId, string deviceId, int enrollId, 
        int fingerIndex, string template);
    Task<bool> EnrollmentExistsAsync(int employeeId, int fingerIndex, string deviceId);
}

public class EnrollmentService : IEnrollmentService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(ApplicationDbContext context, ILogger<EnrollmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Employee?> GetOrCreateEmployeeAsync(string code, string fullName)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Code == code);

            if (employee == null)
            {
                employee = new Employee
                {
                    Code = code,
                    FullName = fullName
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created new employee: {Code} - {FullName}", code, fullName);
            }
            else
            {
                if (employee.FullName != fullName)
                {
                    employee.FullName = fullName;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Updated employee name: {Code} - {FullName}", code, fullName);
                }
            }

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting or creating employee");
            return null;
        }
    }

    public async Task<Employee?> GetEmployeeByIdAsync(int id)
    {
        try
        {
            return await _context.Employees.FindAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee by ID");
            return null;
        }
    }

    public async Task<Enrollment?> SaveEnrollmentAsync(int employeeId, string deviceId, 
        int enrollId, int fingerIndex, string template)
    {
        try
        {
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId 
                    && e.FingerIndex == fingerIndex 
                    && e.DeviceId == deviceId);

            if (existingEnrollment != null)
            {
                existingEnrollment.Template = template;
                existingEnrollment.EnrollId = enrollId;
                existingEnrollment.CreatedAt = DateTime.UtcNow;
                
                await _context.SaveChangesAsync();
                _logger.LogInformation("Updated enrollment for employee {EmployeeId}, finger {FingerIndex}", 
                    employeeId, fingerIndex);
                
                return existingEnrollment;
            }
            else
            {
                var enrollment = new Enrollment
                {
                    EmployeeId = employeeId,
                    DeviceId = deviceId,
                    EnrollId = enrollId,
                    FingerIndex = fingerIndex,
                    Template = template,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Enrollments.Add(enrollment);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Created enrollment for employee {EmployeeId}, finger {FingerIndex}", 
                    employeeId, fingerIndex);
                
                return enrollment;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving enrollment");
            return null;
        }
    }

    public async Task<bool> EnrollmentExistsAsync(int employeeId, int fingerIndex, string deviceId)
    {
        try
        {
            return await _context.Enrollments
                .AnyAsync(e => e.EmployeeId == employeeId 
                    && e.FingerIndex == fingerIndex 
                    && e.DeviceId == deviceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking enrollment existence");
            return false;
        }
    }
}
