using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

public class CreateRecipeRequest
{
    public required string Name { get; set; }
    public required byte[] Image { set; get; }
    public required ICollection<RecipeIngredientDto> RecipeProducts { get; set; }
    public ICollection<RecipeCategoryDto> RecipeCategories { get; set; }
    public required int Portions { get; set; } = 1;

    public string Description { get; set; } = String.Empty;
    public int Duration { get; set; }

    public CreateRecipeStepDto[] Steps { get; set; } = [];

    public string? UserId { get; set; }
}

public class RecipeIngredientDto
{
    public int ProductId { get; set; }
    public float Weight { get; set; }
}

public class CreateRecipeStepDto
{
    public int Order { get; set; }
    public string Description { get; set; }
    public int Timer { get; set; }
    public byte[]? Image { get; set; }
}