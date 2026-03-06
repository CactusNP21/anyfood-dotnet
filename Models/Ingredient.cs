namespace anyfood_dotnet.Models;

public class Ingredient
{
    public int Id { get; set; }
    public string Weight { get; set; }
    public int ProductId { get; set; }
    public Product? Product { get; set; }
    
    public Dish? Dish { get; set; }
    public int DishId { get; set; }
}