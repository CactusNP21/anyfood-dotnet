namespace Domain.Entities;

public class ProductPriceHistory
{
    public int Id { get; set; }
    public required decimal Price { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

    public required int ProductId { get; set; }
    public Product Product { get; set; } = null!;
}