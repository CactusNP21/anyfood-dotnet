using Application.Auth.DTOs;
using Application.Auth.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request)
    {
        var response = await authService.RegisterAsync(request);
        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var response = await authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<LoginResponse>> Refresh([FromBody] string refreshToken)
    {
        var response = await authService.RefreshAsync(refreshToken);
        return Ok(response);
    }

    [HttpPost("revoke")]
    public async Task<IActionResult> Revoke([FromBody] string refreshToken)
    {
        await authService.RevokeAsync(refreshToken);
        return NoContent();
    }
    
    [HttpPost("google")]
    public async Task<ActionResult<LoginResponse>> GoogleLogin(GoogleLoginRequest request)
    {
        var response = await authService.LoginWithGoogleAsync(request);
        return Ok(response);
    }
}