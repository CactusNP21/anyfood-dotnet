using Application.RecipeCategories.DTOs;
using Application.Recipes.DTOs;

namespace API.MultipartFormModels;

public class UpdateRecipeFormRequest
{
    public required string Name { get; set; }
    public IFormFile? Image { get; set; } // не передано — залишається поточне зображення
    public required ICollection<RecipeIngredientDto> RecipeProducts { get; set; }
    public ICollection<RecipeCategoryFormDto> RecipeCategories { get; set; } = [];
    public int? Portions { get; set; } // не використовується в розрахунках; не передано — залишається поточне
    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }
    public UpdateRecipeStepFormDto[] Steps { get; set; } = [];
}

public class UpdateRecipeStepFormDto
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Timer { get; set; }
    public IFormFile? Image { get; set; }
    public string? ImageUrl { get; set; }
}
