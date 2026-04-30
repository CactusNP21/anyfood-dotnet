using System.Security.Claims;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/recipes")]
public class RecipeController(IRecipeService service) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RecipeDto>>> GetAll()
    {
        var recipes = await service.GetAllAsync();
        return Ok(recipes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<RecipeDto>> GetById(int id)
    {
        var recipe = await service.GetByIdAsync(id);
        return Ok(recipe);
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<RecipeDto>> Create(CreateRecipeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        request.UserId = userId;

        var recipe = await service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }

    [HttpPut("{id:int}")]
    [Authorize]
    public async Task<ActionResult<RecipeDto>> Update(int id, UpdateRecipeRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        request.UserId = userId;

        var recipe = await service.UpdateAsync(id, request);
        return Ok(recipe);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id:int}/save")]
    [Authorize]
    public async Task<IActionResult> SaveRecipe(int id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException();
        await service.SaveRecipe(id, userId);
        return NoContent();
    }

    // // ── Версіонування ────────────────────────────────────────────────────────
    //
    // // GET /api/recipes/5/versions — список всіх версій рецепту
    // [HttpGet("{id:int}/versions")]
    // public async Task<ActionResult<IReadOnlyList<RecipeVersionDto>>> GetVersions(int id)
    // {
    //     var versions = await service.GetVersionsAsync(id);
    //     return Ok(versions);
    // }
    //
    // // GET /api/recipes/5/versions/12 — конкретна версія з інгредієнтами
    // [HttpGet("{id:int}/versions/{versionId:int}")]
    // public async Task<ActionResult<RecipeVersionDto>> GetVersion(int id, int versionId)
    // {
    //     var version = await service.GetVersionByIdAsync(id, versionId);
    //     return Ok(version);
    // }
}