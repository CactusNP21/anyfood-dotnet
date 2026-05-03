namespace Application.DayPlans.DTOs;

public class CreateDayPlanRequest
{
    public required string Name { get; set; }
    public required ICollection<DayPlanEntryDto> Entries { get; set; }
    // UserId — з JWT в контролері, як у RecipeController
}

public class DayPlanEntryDto
{
    public int? RecipeId { get; set; }
    public int? ProductId { get; set; }
    public float Weight { get; set; }
}