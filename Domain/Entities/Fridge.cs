namespace Domain.Entities;

public class Fridge
{
    public int Id { get; set; }

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<FridgeItem> Items { get; set; } = [];

}