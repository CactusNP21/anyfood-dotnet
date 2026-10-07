using System.Security.Claims;
using System.Text.Json;
using API.MultipartFormModels;
using Application.RecipeCategories.DTOs;
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
            steps.Add(new CreateRecipeStepDto { Order = s.Order, Description = s.Description, Timer = s.Timer, Image = stepImage });
        }

        var request = new CreateRecipeRequest
        {
            Name = form.Name,
            Image = mainImage,
            RecipeProducts = form.RecipeProducts,
            RecipeCategories = ToCategoryDtos(form.RecipeCategories),
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
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<RecipeDto>> Update(int id, [FromForm] UpdateRecipeFormRequest form)
    {
        var steps = new List<UpdateRecipeStepDto>();
        foreach (var s in form.Steps)
        {
            steps.Add(new UpdateRecipeStepDto
            {
                Order = s.Order,
                Description = s.Description,
                Timer = s.Timer,
                Image = await ReadFileAsync(s.Image),
                ImageUrl = s.ImageUrl,
            });
        }

        var request = new UpdateRecipeRequest
        {
            Name = form.Name,
            Image = await ReadFileAsync(form.Image),
            RecipeProducts = form.RecipeProducts,
            RecipeCategories = ToCategoryDtos(form.RecipeCategories),
            Portions = form.Portions,
            Description = form.Description,
            Duration = form.Duration,
            Steps = steps.ToArray(),
            UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
        };

        var recipe = await service.UpdateAsync(id, request, User.IsInRole("Admin"));
        return Ok(recipe);
    }

    private static List<RecipeCategoryDto> ToCategoryDtos(IEnumerable<RecipeCategoryFormDto> categories)
        => categories.Select(c => new RecipeCategoryDto { Id = c.Id, Name = c.Name ?? string.Empty }).ToList();

    private static async Task<byte[]?> ReadFileAsync(IFormFile? file)
    {
        if (file is null) return null;

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }

    [HttpDelete("{id:int}")]
    [Authorize]
    public async Task<IActionResult> Delete(int id)
    {
        await service.DeleteAsync(id, User.FindFirstValue(ClaimTypes.NameIdentifier), User.IsInRole("Admin"));
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