using System.Security.Claims;
using API.Base;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/recipe")]
public class RecipeController(IRecipeService service)
    : BaseController<IRecipeService, RecipeDto, CreateRecipeRequest, UpdateRecipeRequest>(service)
{
    private readonly IRecipeService _service = service;

    [Authorize]
    [HttpPost("{id:int}")]
    public async Task<IActionResult> SaveRecipe(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? throw new KeyNotFoundException();
        await _service.SaveRecipe(id, userId);
        return NoContent();
    }
};