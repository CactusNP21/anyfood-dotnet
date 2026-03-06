namespace anyfood_dotnet.Models;

public class Dish
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public required string ImageUrl { get; set; }
    public List<Ingredient> Ingredients { get; set; } = new();
}

