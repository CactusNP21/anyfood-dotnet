using System.Security.Claims;
using Application.ShoppingList.DTO;
using Application.ShoppingList.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/shopping-lists")]
[Authorize]
public class ShoppingListController(IShoppingListService service) : ControllerBase
{
    [HttpPost("generate")]
    public async Task<ActionResult<ShoppingListDto>> Generate(GenerateShoppingListRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await service.GenerateAsync(request, userId);
        return Ok(result);
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShoppingListDto>>> GetMy()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var lists = await service.GetByUserAsync(userId);
        return Ok(lists);
    }

    [HttpPatch("{id:int}/items/{itemId:int}/toggle")]
    public async Task<IActionResult> TogglePurchased(int id, int itemId)
    {
        await service.TogglePurchasedAsync(id, itemId);
        return NoContent();
    }
}