using Application.Auth.DTOs;
using Application.Products.DTOs;
using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

public class RecipeDto
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required float Price { get; set; }
    public required string ImageUrl { set; get; }
    public required ICollection<ProductDto> Products { get; set; }
    public required ICollection<RecipeCategoryDto> RecipeCategories { get; set; }
    public required int Portions { get; set; }

    public string Description { get; set; } = String.Empty;
    public int Duration { get; set; }
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }

    public string? UserId { get; set; }
    public UserDto? User { get; set; }
}