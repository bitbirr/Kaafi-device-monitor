using Microsoft.AspNetCore.Mvc;
using Kaafi.DeviceMonitor.Services;

namespace Kaafi.DeviceMonitor.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IJwtService jwtService, ILogger<AuthController> logger)
    {
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpPost("token")]
    public IActionResult GetToken([FromBody] TokenRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Success = false, Message = "Username and password are required" });
            }

            if (ValidateCredentials(request.Username, request.Password))
            {
                var token = _jwtService.GenerateToken(request.Username, request.Username);
                
                return Ok(new TokenResponse
                {
                    Success = true,
                    Token = token,
                    ExpiresIn = 28800
                });
            }
            else
            {
                return Unauthorized(new { Success = false, Message = "Invalid credentials" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating token");
            return StatusCode(500, new { Success = false, Message = $"Internal error: {ex.Message}" });
        }
    }

    private bool ValidateCredentials(string username, string password)
    {
        return username == "biotime" && password == "biotime9.5";
    }
}

public class TokenRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class TokenResponse
{
    public bool Success { get; set; }
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
}
