using Application.RecipeCategories.DTOs;
using Application.Recipes.DTOs;

namespace API.MultipartFormModels;

public class CreateRecipeFormRequest
{
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public IFormFile? Image { get; set; }
    public required ICollection<RecipeIngredientDto> RecipeProducts { get; set; }
    public ICollection<RecipeCategoryDto> RecipeCategories { get; set; } = [];
    public required int Portions { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }
    public RecipeStepFormDto[] Steps { get; set; } = [];
}

public class RecipeStepFormDto
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
}