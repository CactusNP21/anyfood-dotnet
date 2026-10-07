using Application.RecipeCategories.DTOs;

namespace Application.Recipes.DTOs;

// Повна заміна рецепту: інгредієнти, категорії та кроки перезаписуються списками з запиту
public class UpdateRecipeRequest
{
    public required string Name { get; set; }
    public byte[]? Image { get; set; } // null — залишити поточне головне зображення
    public required ICollection<RecipeIngredientDto> RecipeProducts { get; set; }
    public ICollection<RecipeCategoryDto> RecipeCategories { get; set; } = [];
    public int? Portions { get; set; } // null — залишити поточне значення

    public string Description { get; set; } = string.Empty;
    public int Duration { get; set; }

    public UpdateRecipeStepDto[] Steps { get; set; } = [];

    public string? UserId { get; set; }
}

public class UpdateRecipeStepDto
{
    public int Order { get; set; }
    public string Description { get; set; } = string.Empty;
    public int Timer { get; set; }
    public byte[]? Image { get; set; }    // новий файл — має пріоритет над ImageUrl
    public string? ImageUrl { get; set; } // наявне зображення кроку цього рецепту, яке треба залишити
}
