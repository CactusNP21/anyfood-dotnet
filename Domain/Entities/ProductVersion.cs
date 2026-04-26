namespace Domain.Entities;

public class ProductVersion
{
    public int Id { get; set; }

    // Посилання на продукт якому належить ця версія
    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;
    
    // Порядковий номер: 1, 2, 3...
    public int VersionNumber { get; set; }

    // ── Повний snapshot продукту на момент збереження ────────────────────────
    public string Name { get; set; } = string.Empty;
    public decimal Calories { get; set; }
    public decimal Protein { get; set; }
    public decimal Fat { get; set; }
    public decimal Carbs { get; set; }
    public decimal Price { get; set; }
    public int? GlycemicIndex { get; set; }
    public string? ImageUrl { get; set; }
    public int CategoryId { get; set; }

    // ── Мета ────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

}