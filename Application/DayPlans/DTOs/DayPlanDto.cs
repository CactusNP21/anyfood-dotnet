namespace Application.DayPlans.DTOs;

public class DayPlanDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = null!;
    public ICollection<DayPlanEntryResultDto> Entries { get; set; } = [];
}

public class DayPlanEntryResultDto
{
    public int Id { get; set; }
    public float Weight { get; set; }
    public int? RecipeId { get; set; }
    public int? ProductId { get; set; }
}