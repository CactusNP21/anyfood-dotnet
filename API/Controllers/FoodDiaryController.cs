using System.Security.Claims;
using Application.FoodDiary.DTOs;
using Application.FoodDiary.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/diary")]
[Authorize]
public class FoodDiaryController(IFoodDiaryService service) : ControllerBase
{
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)
                             ?? throw new UnauthorizedAccessException();

    // GET api/diary?from=2026-09-01&to=2026-09-30
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodDiaryDaySummaryDto>>> GetRange(
        [FromQuery] DateOnly from, [FromQuery] DateOnly to)
    {
        var days = await service.GetRangeAsync(UserId, from, to);
        return Ok(days);
    }

    // GET api/diary/2026-09-14
    [HttpGet("{date}")]
    public async Task<ActionResult<FoodDiaryDayDto>> GetDay(DateOnly date)
    {
        var day = await service.GetDayAsync(UserId, date);
        return Ok(day);
    }

    // Повністю замінює записи дня; порожній масив очищає день
    [HttpPut("{date}/entries")]
    public async Task<ActionResult<FoodDiaryDayDto>> ReplaceEntries(
        DateOnly date, ICollection<FoodDiaryEntryRequest> entries)
    {
        var day = await service.ReplaceEntriesAsync(UserId, date, entries);
        return Ok(day);
    }

    [HttpPut("{date}/goal")]
    public async Task<ActionResult<FoodDiaryDayDto>> SetDayGoal(DateOnly date, SetDayCalorieGoalRequest request)
    {
        var day = await service.SetDayGoalAsync(UserId, date, request);
        return Ok(day);
    }

    // Повертає день до цілі за замовчуванням
    [HttpDelete("{date}/goal")]
    public async Task<ActionResult<FoodDiaryDayDto>> ClearDayGoal(DateOnly date)
    {
        var day = await service.ClearDayGoalAsync(UserId, date);
        return Ok(day);
    }

    // Нова ціль за замовчуванням з EffectiveFrom; попередні дні не змінюються
    [HttpPut("goal")]
    public async Task<IActionResult> SetDefaultGoal(SetCalorieGoalRequest request)
    {
        await service.SetDefaultGoalAsync(UserId, request);
        return NoContent();
    }
}
