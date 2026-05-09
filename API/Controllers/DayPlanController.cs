using System.Security.Claims;
using Application.DayPlans.DTOs;
using Application.DayPlans.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/day-plan")]
public class DayPlanController(IDayPlanService service) : ControllerBase
{
    // ── Публічні ─────────────────────────────────────────────────────────────

    [HttpGet("{id:int}")]
    public async Task<ActionResult<DayPlanDto>> GetById(int id)
    {
        var dayPlan = await service.GetByIdAsync(id);
        return Ok(dayPlan);
    }

    // ── Авторизовані ─────────────────────────────────────────────────────────

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<DayPlanDto>>> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException();

        var plans = await service.GetByUserAsync(userId);
        return Ok(plans);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<DayPlanDto>> Create(CreateDayPlanRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException();

        var dayPlan = await service.CreateAsync(request, userId);
        return CreatedAtAction(nameof(GetById), new { id = dayPlan.Id }, dayPlan);
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }
}