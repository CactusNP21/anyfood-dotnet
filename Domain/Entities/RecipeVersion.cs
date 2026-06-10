namespace Domain.Entities;

public class RecipeVersion
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int VersionNumber { get; set; }

    // ── Snapshot основних полів рецепту ─────────────────────────────────────
    public required string Name { get; set; } = string.Empty;
    public required string Description { get; set; } = string.Empty;
    public required string ImageUrl { get; set; } = string.Empty;
    public required int Portions { get; set; }
    public required int Duration { get; set; }

    // ── Snapshot розрахованих БЖВ (на 100г рецепту) ─────────────────────────
    public required float Calories { get; set; }
    public required float Protein { get; set; }
    public required float Fat { get; set; }
    public required float Carbs { get; set; }
    public required float Price { get; set; }

    // ── Інгредієнти цієї версії ──────────────────────────────────────────────
    public ICollection<RecipeVersionIngredient> Ingredients { get; set; } = [];

    // ── Мета ────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}