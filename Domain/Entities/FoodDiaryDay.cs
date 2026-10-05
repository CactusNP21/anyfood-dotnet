namespace Domain.Entities;

public class FoodDiaryDay
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateOnly Date { get; set; }

    // Ціль, змінена вручну саме для цього дня. null — береться з CalorieGoal, що діяла на цю дату
    public int? CalorieGoalOverride { get; set; }

    public ICollection<FoodDiaryEntry> Entries { get; set; } = [];
}
