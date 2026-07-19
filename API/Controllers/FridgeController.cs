// API/Controllers/FridgeController.cs
using System.Security.Claims;
using Application.Fridge.DTOs;
using Application.Fridge.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/fridge")]
[Authorize]
public class FridgeController(IFridgeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<FridgeDto>> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await service.GetMyFridgeAsync(userId));
    }

    [HttpPost("from-shopping-list/{shoppingListId:int}")]
    public async Task<IActionResult> AddFromShoppingList(int shoppingListId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        await service.AddFromShoppingListAsync(shoppingListId, userId);
        return NoContent();
    }

    [HttpPost("consume")]
    public async Task<ActionResult<ConsumeRecipeResultDto>> Consume(ConsumeRecipeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        return Ok(await service.ConsumeForRecipeAsync(request, userId));
    }
}