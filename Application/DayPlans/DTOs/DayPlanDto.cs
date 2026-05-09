namespace Application.DayPlans.DTOs;

public class DayPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string UserId { get; set; } = null!;
    public ICollection<DayPlanEntryResultDto> Entries { get; set; } = [];
}

public class DayPlanEntryResultDto
{
    public int Id { get; set; }
    public float Weight { get; set; }
    public int? RecipeVersionId { get; set; }
    public int? ProductVersionId { get; set; }
}