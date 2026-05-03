namespace Domain.Entities;

public class DayPlanEntry
{
    public int Id { get; set; }

    public int DayPlanId { get; set; }
    public DayPlan DayPlan { get; set; } = null!;

    // Або рецепт, або продукт — один із двох
    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Вага в грамах (порція)
    public float Weight { get; set; }
}