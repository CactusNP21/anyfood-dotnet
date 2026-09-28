using System.Security.Claims;
using System.Text.Json;
using API.MultipartFormModels;
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
    public async Task<ActionResult<RecipeDto>> Create([FromForm] CreateRecipeFormRequest form)
    {
        byte[] mainImage = [];
        if (form.Image is not null)
        {
            using var ms = new MemoryStream();
            await form.Image.CopyToAsync(ms);
            mainImage = ms.ToArray();
        }

        var steps = new List<CreateRecipeStepDto>();
        foreach (var s in form.Steps)
        {
            byte[]? stepImage = null;
            if (s.Image is not null)
            {
                using var ms = new MemoryStream();
                await s.Image.CopyToAsync(ms);
                stepImage = ms.ToArray();
            }
            steps.Add(new CreateRecipeStepDto { Order = s.Order, Description = s.Description, Image = stepImage });
        }

        var request = new CreateRecipeRequest
        {
            Name = form.Name,
            Image = mainImage,
            RecipeProducts = form.RecipeProducts,
            RecipeCategories = form.RecipeCategories,
            Portions = form.Portions,
            Description = form.Description,
            Duration = form.Duration,
            Steps = steps.ToArray(),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        };

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