namespace Domain.Entities;

public class SavedProduct
{
    public required string UserId { get; set; }
    public User User { get; set; } = null!;

    public required int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}