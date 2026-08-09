using System.Security.Claims;
using System.Text.Json;
using Application.Recipes.DTOs;
using Application.Recipes.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/recipe")]
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<RecipeDto>> Create([FromForm] CreateRecipeRequest request)
    {
        request.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var recipe = await service.CreateAsync(request, User.IsInRole("Admin"));
        return CreatedAtAction(nameof(GetById), new { id = recipe.Id }, recipe);
    }
    
    // [HttpGet("filter")]
    // public async Task<ActionResult<IReadOnlyList<RecipeSummaryDto>>> Filter([FromQuery] ProductFilterRequest filter)
    // {
    //     var products = await productService.FilterAsync(filter);
    //     return Ok(products);
    // }

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
}