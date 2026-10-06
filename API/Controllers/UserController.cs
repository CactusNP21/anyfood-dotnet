using System.Security.Claims;
using Application.Users.DTOs;
using Application.Users.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService userService;

    public UsersController(IUserService userService)
    {
        this.userService = userService;
    }

    [HttpGet("self")]
    public async Task<IActionResult> GetProfile()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var profile = await userService.GetProfileAsync(userId);
        return Ok(profile);
    }

    // Повна заміна параметрів тіла; updatedAt ставить сервер
    [HttpPut("self/body-params")]
    public async Task<ActionResult<BodyParamsDto>> UpdateBodyParams(BodyParamsRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var bodyParams = await userService.UpdateBodyParamsAsync(userId, request);
        return Ok(bodyParams);
    }

    // Очищає параметри тіла; цілі калорій не змінюються
    [HttpDelete("self/body-params")]
    public async Task<IActionResult> ClearBodyParams()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        await userService.ClearBodyParamsAsync(userId);
        return NoContent();
    }
}
