namespace Domain.Entities;

public class SavedRecipe
{
    public required int RecipeVersionId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public required string UserId { get; set; }

}