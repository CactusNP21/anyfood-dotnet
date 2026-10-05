namespace Domain.Entities;

// Історія цілей: кожна зміна — новий запис, діє з EffectiveFrom до наступного запису
public class CalorieGoal
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public DateOnly EffectiveFrom { get; set; }
    public int Calories { get; set; }
}
