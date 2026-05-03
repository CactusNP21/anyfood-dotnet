namespace Domain.Entities;

public class DayPlan
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<DayPlanEntry> Entries { get; set; } = [];
}