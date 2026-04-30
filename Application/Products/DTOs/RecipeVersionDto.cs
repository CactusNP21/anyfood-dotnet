using Application.Products.DTOs;

namespace Application.Recipes.DTOs;

public class RecipeVersionDto
{
    public int Id { get; set; }
    public int RecipeId { get; set; }
    public int VersionNumber { get; set; }

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Portions { get; set; }
    public int Duration { get; set; }

    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
    public float Price { get; set; }

    // Інгредієнти з даними продукту на момент збереження версії
    public ICollection<RecipeVersionIngredientDto> Ingredients { get; set; } = [];

    public DateTime CreatedAt { get; set; }
    public string? CreatedByUserId { get; set; }
}

public class RecipeVersionIngredientDto
{
    public int Id { get; set; }
    public float Weight { get; set; }

    // Дані продукту на момент збереження — з ProductVersion
    public ProductVersionDto ProductVersion { get; set; } = null!;
}