namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Carbs { get; set; }
    public int? GlycemicIndex { get; set; }
    public string? ImageUrl { get; set; }
    public decimal Price { get; set; }
    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = [];
    public bool IsSystem { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? UserId { get; set; }
    public User? User { get; set; }
}