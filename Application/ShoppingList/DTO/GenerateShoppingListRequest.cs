// Application/ShoppingLists/DTOs/GenerateShoppingListRequest.cs

namespace Application.ShoppingList.DTO;

public class GenerateShoppingListRequest
{
    public required string Name { get; set; }

    // Будь-яка комбінація джерел
    public ICollection<RecipeSource> Recipes { get; set; } = [];
    public ICollection<ProductSource> Products { get; set; } = [];
    public ICollection<int> DayPlanIds { get; set; } = [];
}

public class RecipeSource
{
    public int RecipeVersionId { get; set; }
    public int Weight { get; set; } = 1;
}

public class ProductSource
{
    public int ProductVersionId { get; set; }
    public float Weight { get; set; }
}