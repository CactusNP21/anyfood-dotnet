using Application.Products.DTOs;
using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

public class UpdateRecipeRequest
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required string ImageUrl { set; get; }
    public required ICollection<ProductDto> Products { get; set; }
    public required ICollection<RecipeCategoryDto> RecipeCategories { get; set; }
    public required int Portions { get; set; }

    public string Description { get; set; } = String.Empty;
    public int Duration { get; set; }
    
    public string? UserId { get; set; }

}