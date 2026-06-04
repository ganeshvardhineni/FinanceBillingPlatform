using FinanceBilling.API.DTOs;
using FinanceBilling.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceBilling.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Hardcoded users for demo — in production this would hit the DB
        var users = new Dictionary<string, (string Password, string Role)>
        {
            { "admin",   ("Admin@123",   "Admin") },
            { "manager", ("Manager@123", "Manager") },
            { "viewer",  ("Viewer@123",  "Viewer") }
        };

        if (!users.TryGetValue(request.Username.ToLower(), out var user))
            return Unauthorized(new { error = "Invalid username or password." });

        if (user.Password != request.Password)
            return Unauthorized(new { error = "Invalid username or password." });

        var token = _jwtService.GenerateToken(request.Username, user.Role);

        return Ok(new LoginResponseDto
        {
            Token = token,
            Username = request.Username,
            Role = user.Role,
            ExpiresAt = DateTime.UtcNow.AddMinutes(60)
        });
    }
}