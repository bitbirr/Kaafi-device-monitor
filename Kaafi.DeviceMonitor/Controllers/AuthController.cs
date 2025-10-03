using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Kaafi.DeviceMonitor.Models;


namespace Kaafi.DeviceMonitor.Controllers;

/// <summary>
/// Authentication controller for user login and registration
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly IConfiguration _configuration;

    public AuthController(
        UserManager<IdentityUser> userManager,
        SignInManager<IdentityUser> signInManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration request containing email and password</param>
    /// <returns>Registration result with success status</returns>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Invalid registration data"
            });
        }

        var user = new IdentityUser
        {
            UserName = request.Email,
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (result.Succeeded)
        {
            return Ok(new AuthResponse
            {
                Success = true,
                Message = "User registered successfully",
                Email = user.Email
            });
        }

        return BadRequest(new AuthResponse
        {
            Success = false,
            Message = string.Join(", ", result.Errors.Select(e => e.Description))
        });
    }

    /// <summary>
    /// Authenticate user and return JWT token
    /// </summary>
    /// <param name="request">Login request containing email and password</param>
    /// <returns>JWT token and user information on successful login</returns>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(new AuthResponse
            {
                Success = false,
                Message = "Invalid login data"
            });
        }

        var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, false, false);

        if (result.Succeeded)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            var token = GenerateJwtToken(user!);

            return Ok(new AuthResponse
            {
                Success = true,
                Token = token,
                Email = user.Email!,
                Message = "Login successful"
            });
        }

        return Unauthorized(new AuthResponse
        {
            Success = false,
            Message = "Invalid email or password"
        });
    }

    private string GenerateJwtToken(IdentityUser user)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"] ?? "KaafiDeviceMonitor",
            audience: _configuration["Jwt:Audience"] ?? "KaafiDeviceMonitor",
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}