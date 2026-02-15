namespace anyfood_dotnet.Models;

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Навігаційна властивість: одна категорія може мати багато продуктів
    public ICollection<Ingredient> Foods { get; set; } = new List<Ingredient>();
}