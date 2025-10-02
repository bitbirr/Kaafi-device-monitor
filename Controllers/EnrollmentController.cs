using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Kaafi.DeviceMonitor.Models.DTOs;
using Kaafi.DeviceMonitor.Services;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrollmentController : ControllerBase
{
    private readonly IZKTecoService _zkService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ILogger<EnrollmentController> _logger;
    private static readonly Dictionary<int, int> _captureProgress = new();

    public EnrollmentController(
        IZKTecoService zkService,
        IEnrollmentService enrollmentService,
        ILogger<EnrollmentController> logger)
    {
        _zkService = zkService;
        _enrollmentService = enrollmentService;
        _logger = logger;
    }

    [HttpPost("start-enroll")]
    public async Task<IActionResult> StartEnroll([FromBody] StartEnrollRequest request)
    {
        try
        {
            if (!_zkService.IsConnected())
            {
                return BadRequest(new StartEnrollResponse
                {
                    Success = false,
                    Message = "Device is not connected. Please connect first."
                });
            }

            if (string.IsNullOrEmpty(request.EmployeeCode) || string.IsNullOrEmpty(request.FullName))
            {
                return BadRequest(new StartEnrollResponse
                {
                    Success = false,
                    Message = "Employee code and full name are required"
                });
            }

            if (request.FingerIndex < 0 || request.FingerIndex > 9)
            {
                return BadRequest(new StartEnrollResponse
                {
                    Success = false,
                    Message = "Finger index must be between 0 and 9"
                });
            }

            var employee = await _enrollmentService.GetOrCreateEmployeeAsync(
                request.EmployeeCode, request.FullName);

            if (employee == null)
            {
                return StatusCode(500, new StartEnrollResponse
                {
                    Success = false,
                    Message = "Failed to create or retrieve employee"
                });
            }

            var deviceId = _zkService.GetDeviceSerialNumber();
            var enrollmentExists = await _enrollmentService.EnrollmentExistsAsync(
                employee.Id, request.FingerIndex, deviceId);

            if (enrollmentExists)
            {
                _logger.LogInformation("Enrollment already exists for employee {EmployeeId}, finger {FingerIndex}. Will update.", 
                    employee.Id, request.FingerIndex);
            }

            var started = _zkService.BeginEnrollment(employee.Id, request.FingerIndex);
            
            if (started)
            {
                _captureProgress[employee.Id] = 0;
                
                return Ok(new StartEnrollResponse
                {
                    Success = true,
                    Message = "Enrollment started successfully. Please place finger 3 times.",
                    EmployeeId = employee.Id
                });
            }
            else
            {
                return StatusCode(500, new StartEnrollResponse
                {
                    Success = false,
                    Message = "Failed to start enrollment on device"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting enrollment");
            return StatusCode(500, new StartEnrollResponse
            {
                Success = false,
                Message = $"Internal error: {ex.Message}"
            });
        }
    }

    [HttpPost("capture-fingerprint")]
    public IActionResult CaptureFingerprint([FromBody] CaptureRequest request)
    {
        try
        {
            if (!_zkService.IsConnected())
            {
                return BadRequest(new CaptureResponse
                {
                    Success = false,
                    Message = "Device is not connected"
                });
            }

            if (request.CaptureNumber < 1 || request.CaptureNumber > 3)
            {
                return BadRequest(new CaptureResponse
                {
                    Success = false,
                    Message = "Capture number must be between 1 and 3"
                });
            }

            var captured = _zkService.CaptureFingerprint(request.CaptureNumber, out string template);
            
            if (captured)
            {
                if (_captureProgress.ContainsKey(request.EmployeeId))
                {
                    _captureProgress[request.EmployeeId] = request.CaptureNumber;
                }
                
                var isComplete = request.CaptureNumber >= 3;
                
                return Ok(new CaptureResponse
                {
                    Success = true,
                    Message = $"Fingerprint {request.CaptureNumber} captured successfully",
                    CaptureCount = request.CaptureNumber,
                    IsComplete = isComplete,
                    Template = isComplete ? template : null
                });
            }
            else
            {
                return BadRequest(new CaptureResponse
                {
                    Success = false,
                    Message = "Failed to capture fingerprint. Please try again."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error capturing fingerprint");
            return StatusCode(500, new CaptureResponse
            {
                Success = false,
                Message = $"Internal error: {ex.Message}"
            });
        }
    }

    [HttpPost("save-enroll")]
    public async Task<IActionResult> SaveEnroll([FromBody] SaveEnrollRequest request)
    {
        try
        {
            if (!_zkService.IsConnected())
            {
                return BadRequest(new SaveEnrollResponse
                {
                    Success = false,
                    Message = "Device is not connected"
                });
            }

            if (string.IsNullOrEmpty(request.Template))
            {
                return BadRequest(new SaveEnrollResponse
                {
                    Success = false,
                    Message = "Template is required"
                });
            }

            var employee = await _enrollmentService.GetEmployeeByIdAsync(request.EmployeeId);
            if (employee == null)
            {
                return NotFound(new SaveEnrollResponse
                {
                    Success = false,
                    Message = "Employee not found"
                });
            }

            var saved = _zkService.SaveTemplate(request.EmployeeId, request.FingerIndex, request.Template);
            
            if (!saved)
            {
                return StatusCode(500, new SaveEnrollResponse
                {
                    Success = false,
                    Message = "Failed to save template to device"
                });
            }

            var deviceId = _zkService.GetDeviceSerialNumber();
            var enrollment = await _enrollmentService.SaveEnrollmentAsync(
                request.EmployeeId,
                deviceId,
                request.EmployeeId,
                request.FingerIndex,
                request.Template
            );

            if (enrollment == null)
            {
                return StatusCode(500, new SaveEnrollResponse
                {
                    Success = false,
                    Message = "Failed to save enrollment to database"
                });
            }

            _captureProgress.Remove(request.EmployeeId);

            return Ok(new SaveEnrollResponse
            {
                Success = true,
                Message = "Enrollment saved successfully",
                EnrollmentId = enrollment.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving enrollment");
            return StatusCode(500, new SaveEnrollResponse
            {
                Success = false,
                Message = $"Internal error: {ex.Message}"
            });
        }
    }
}
