namespace Domain.Entities;

public class RecipeProduct
{
    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public float Weight { get; set; } // grams, for example

}