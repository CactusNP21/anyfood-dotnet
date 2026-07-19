namespace Domain.Entities;

public class DayPlanEntry
{
    public int Id { get; set; }

    public int DayPlanId { get; set; }
    public DayPlan DayPlan { get; set; } = null!;

    public int? RecipeId { get; set; }
    public Recipe? Recipe { get; set; }

    public int? ProductId { get; set; }
    public Product? Product { get; set; }

    // Вага в грамах
    public float Weight { get; set; }
    public required short Time { get; set; }
}