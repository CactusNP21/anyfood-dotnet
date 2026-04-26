namespace Domain.Entities;

public class RecipeVersionIngredient
{
    public int Id { get; set; }

    public int RecipeVersionId { get; set; }
    public RecipeVersion RecipeVersion { get; set; } = null!;

    public int ProductVersionId { get; set; }
    public ProductVersion ProductVersion { get; set; } = null!;

    public float Weight { get; set; }

}