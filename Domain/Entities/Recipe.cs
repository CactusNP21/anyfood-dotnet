namespace Domain.Entities;

public class Recipe
{
    public required int Id { get; set; }
    public required string Name { get; set; }
    public required float Price { get; set; }
    public required string ImageUrl { set; get; }
    public required ICollection<RecipeProduct> RecipeProducts { get; set; }
    public required ICollection<RecipeCategory> RecipeCategories { get; set; }
    public required int Portions { get; set; }
    
    public int? LatestVersionId { get; set; } 
    public RecipeVersion? LatestVersion { get; set; } 
    
    public ICollection<RecipeVersion> Versions { get; set; } = [];
    
    public string Description { get; set; } = String.Empty;
    public int Duration { get; set; }
    public float Calories { get; set; }
    public float Protein { get; set; }
    public float Fat { get; set; }
    public float Carbs { get; set; }

    public string? UserId { get; set; }
    public User? User { get; set; }
}