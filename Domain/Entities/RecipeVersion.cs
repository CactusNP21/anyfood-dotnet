namespace Domain.Entities;

public class RecipeVersion
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int VersionNumber { get; set; }

    // ── Snapshot основних полів рецепту ─────────────────────────────────────
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public int Portions { get; set; }
    public int Duration { get; set; }

    // ── Snapshot розрахованих БЖВ (на 100г рецепту) ─────────────────────────
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }
    public float Price { get; set; }

    // ── Інгредієнти цієї версії ──────────────────────────────────────────────
    public ICollection<RecipeVersionIngredient> Ingredients { get; set; } = [];

    // ── Мета ────────────────────────────────────────────────────────────────
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }
}