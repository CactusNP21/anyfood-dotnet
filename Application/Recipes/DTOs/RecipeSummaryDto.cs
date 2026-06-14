using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

public class RecipeSummaryDto
{
    public required int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required decimal Calories { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Price { get; set; }
    public required string ImageUrl { get; set; } = string.Empty;
    public ICollection<RecipeCategoryDto> Categories { get; set; } = [];
}