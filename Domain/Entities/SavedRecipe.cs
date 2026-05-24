namespace Domain.Entities;

public class SavedRecipe
{
    public required int RecipeVersionId { get; set; }
    public RecipeVersion RecipeVersion { get; set; } = null!;

    public required string UserId { get; set; }

}