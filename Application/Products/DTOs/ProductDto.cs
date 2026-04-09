using Domain.Entities;

namespace Application.Products.DTOs;

public class ProductDto
{
    public required int Id { get; set; }
    public required string Name { get; set; } = string.Empty;
    public required decimal Calories { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Carbs { get; set; }
    public int? GlycemicIndex { get; set; }
    public string? ImageUrl { get; set; }
    public required decimal Price { get; set; }
    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = [];
    public bool IsSystem { get; set; }

    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;

    public string? UserId { get; set; }
    public User? User { get; set; }
}