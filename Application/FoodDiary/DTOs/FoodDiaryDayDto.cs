namespace Application.FoodDiary.DTOs;

public class FoodDiaryDayDto
{
    public DateOnly Date { get; set; }
    public int? CalorieGoal { get; set; }
    public bool IsGoalOverridden { get; set; }
    public float TotalCalories { get; set; }
    public float TotalProtein { get; set; }
    public float TotalFat { get; set; }
    public float TotalCarbs { get; set; }

    public ICollection<FoodDiaryEntryDto> Entries { get; set; } = [];
}

public class FoodDiaryEntryDto
{
    public int Id { get; set; }
    public int? ProductId { get; set; }
    public int? RecipeId { get; set; }
    public required string Name { get; set; }
    public string? ImageUrl { get; set; }
    public float Weight { get; set; }
    public short Time { get; set; }
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
}

public class FoodDiaryDaySummaryDto
{
    public DateOnly Date { get; set; }
    public int? CalorieGoal { get; set; }
    public bool IsGoalOverridden { get; set; }
    public float TotalCalories { get; set; }
    public float TotalProtein { get; set; }
    public float TotalFat { get; set; }
    public float TotalCarbs { get; set; }
}
