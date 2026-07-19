// Application/Fridge/DTOs/ConsumeRecipeRequest.cs
namespace Application.Fridge.DTOs;

public class ConsumeRecipeRequest
{
    public required int RecipeId { get; set; }
    // 1.0 = приготував весь рецепт як є; 0.5 = половину порцій тощо
    public float ServingsMultiplier { get; set; } = 1;
}

public class ConsumeRecipeResultDto
{
    // Продукти, яких не вистачило (фактично забрано менше ніж треба)
    public ICollection<FridgeShortfallDto> Shortfalls { get; set; } = [];
}

public class FridgeShortfallDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public float MissingWeight { get; set; } // скільки не вистачило (AP)
}