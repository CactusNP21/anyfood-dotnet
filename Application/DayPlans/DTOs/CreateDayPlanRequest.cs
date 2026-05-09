namespace Application.DayPlans.DTOs;

public class CreateDayPlanRequest
{
    public required string Name { get; set; }
    public required ICollection<DayPlanEntryDto> Entries { get; set; }
}

public class DayPlanEntryDto
{
    public int? RecipeVersionId { get; set; }
    public int? ProductVersionId { get; set; }
    public float Weight { get; set; }
}