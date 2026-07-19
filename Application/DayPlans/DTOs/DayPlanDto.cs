namespace Application.DayPlans.DTOs;

public class DayPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public required float TotalPrice { get; set; }
    public required float TotalCalories { get; set; }
    public required float TotalCarbs { get; set; }
    public required float TotalFats { get; set; }
    public required float TotalProtein { get; set; }
    
    public ICollection<DayPlanEntryResultDto> Entries { get; set; } = [];
}

public class DayPlanEntryResultDto
{
    public int Id { get; set; }
    public float Weight { get; set; }
    public required int  Time { get; set; }
    public int? RecipeId { get; set; }
    public int? ProductVersionId { get; set; }
    public required string Name { get; set; }
}