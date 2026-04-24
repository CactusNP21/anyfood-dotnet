using Application.Products.DTOs;
using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

public class CreateRecipeRequest
{
    public required string Name { get; set; }
    public required string ImageUrl { set; get; }
    public required ICollection<RecipeIngredientDto> RecipeProducts { get; set; }
    public ICollection<RecipeCategoryDto> RecipeCategories { get; set; }
    public required int Portions { get; set; }

    public string Description { get; set; } = String.Empty;
    public int Duration { get; set; }
    
    public string? UserId { get; set; }
}

public class RecipeIngredientDto
{
    public int ProductId { get; set; }
    public float Weight { get; set; }
}