namespace anyfood_dotnet.Dtos;

public class IngredientDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    
    // Ми беремо лише назву категорії, а не весь об'єкт
    public string CategoryName { get; set; } = string.Empty;

    public double Calories { get; set; }
    public double Protein { get; set; }
    public double Fat { get; set; }
    public double Carbs { get; set; }
}