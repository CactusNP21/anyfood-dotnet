namespace Application.Products.DTOs;

public class ProductVersionDto
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public int VersionNumber { get; set; }

    public string Name { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Carbs { get; set; }
    public decimal Price { get; set; }
    public int? GlycemicIndex { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }

    public DateTime CreatedAt { get; set; }
}