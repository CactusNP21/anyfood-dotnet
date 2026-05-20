namespace Domain.Entities;

public class RecipeVersionIngredient
{
    public int Id { get; set; }

    public int RecipeVersionId { get; set; }
    public RecipeVersion RecipeVersion { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public float Weight { get; set; }

}