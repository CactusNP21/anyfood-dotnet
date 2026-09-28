using Domain.Common;

namespace Domain.Entities;

public class Product
{
    public int Id { get; set; }
    public required string Name { get; set; } = string.Empty;

    // ── Власні (origin) значення — зберігаються завжди, редагуються лише листками ──
    public decimal OwnCalories { get; set; }
    public decimal OwnProtein { get; set; }
    public decimal OwnFat { get; set; }
    public decimal OwnCarbs { get; set; }
    public decimal OwnPrice { get; set; }
    public decimal OwnSalt { get; set; } = 0;
    public int? OwnGlycemicIndex { get; set; }

    // ── Ефективні значення — те, що читають рецепти/списки покупок ──
    // Дорівнюють Own*, якщо немає дітей; інакше — avg(Children.*)
    public required decimal Calories { get; set; }
    public required decimal Protein { get; set; }
    public required decimal Fat { get; set; }
    public required decimal Carbs { get; set; }
    public required decimal Salt { get; set; }
    public int? GlycemicIndex { get; set; }
    public required decimal Price { get; set; }
    public required float EdiblePortionFactor { get; set; } = 1;

    public string? ImageUrl { get; set; }
    public ICollection<ProductPriceHistory> PriceHistory { get; set; } = [];
    public bool IsSystem { get; set; }
    public ICollection<Category> Categories { get; set; } = [];
    public string? UserId { get; set; }
    public User? User { get; set; }

    // ── Ієрархія ──────────────────────────────────────────────────────────
    public int? ParentProductId { get; set; }
    public Product? ParentProduct { get; set; }
    public ICollection<Product> Children { get; set; } = [];
    public string Path { get; set; } = string.Empty;
}