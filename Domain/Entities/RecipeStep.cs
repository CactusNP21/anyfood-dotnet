namespace Domain.Entities;

public class RecipeStep
{
    public int Id { get; set; }

    public int RecipeId { get; set; }
    public Recipe Recipe { get; set; } = null!;

    public int Order { get; set; }
    
    public required string Description { get; set; }
    
    public int Timer { get; set; }
    
    public string? ImageUrl { get; set; }
    
}