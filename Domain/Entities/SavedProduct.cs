namespace Domain.Entities;

public class SavedProduct
{
    public required string UserId { get; set; }
    public User User { get; set; } = null!;

    public required int ProductVersionId { get; set; }
    public ProductVersion ProductVersion { get; set; } = null!;
}