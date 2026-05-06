using Microsoft.AspNetCore.Mvc;
using QuickMind.Application.DTOs;
using QuickMind.Application.Services;

namespace QuickMind.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService) => _authService = authService;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginDto dto)
    {
        try
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { error = ex.Message });
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult<LoginResponseDto>> Register(RegisterDto dto)
    {
        try
        {
            var result = await _authService.RegisterAsync(dto);
            return Created(string.Empty, result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { error = ex.Message });
        }
    }

    [HttpPost("guest")]
    public async Task<ActionResult<LoginResponseDto>> GuestLogin([FromBody] string nickname)
    {
        var result = await _authService.GuestLoginAsync(nickname);
        return Ok(result);
    }

    [HttpGet("profile/{userId}")]
    public async Task<ActionResult<UserDto>> GetProfile(Guid userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        return user == null ? NotFound(new { error = "Usuario no encontrado" }) : Ok(user);
    }

    [HttpPut("profile/{userId}")]
    public async Task<ActionResult<UserDto>> UpdateProfile(Guid userId, UpdateProfileDto dto)
    {
        try
        {
            var user = await _authService.UpdateProfileAsync(userId, dto);
            return Ok(user);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}
