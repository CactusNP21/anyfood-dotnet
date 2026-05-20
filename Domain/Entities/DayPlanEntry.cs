namespace Domain.Entities;

public class DayPlanEntry
{
    public int Id { get; set; }

    public int DayPlanId { get; set; }
    public DayPlan DayPlan { get; set; } = null!;

    // Або версія рецепту, або версія продукту — рівно один із двох
    public int? RecipeVersionId { get; set; }
    public RecipeVersion? RecipeVersion { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Вага в грамах
    public float Weight { get; set; }
}