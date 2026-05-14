namespace Application.DayPlans.DTOs;

public class CreateDayPlanRequest
{
    public required string Name { get; set; }
    public required ICollection<DayPlanEntryDto> Entries { get; set; }
}

public class DayPlanEntryDto
{
    public int? ProductId { get; set; }      // замість ProductVersionId
    public int? RecipeId { get; set; } 
    public float Weight { get; set; }
}