namespace Application.ShoppingList.DTO;

public class ShoppingListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<ShoppingListGroupDto> Groups { get; set; } = [];
    public decimal TotalPrice { get; set; }
}

public class ShoppingListGroupDto
{
    public int ProductId { get; set; }
    public string GroupName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public float TotalWeight { get; set; }
    public decimal TotalPrice { get; set; }

    // Дочірні групи — присутні лише якщо під цим вузлом є розгалуження
    public ICollection<ShoppingListGroupDto> SubGroups { get; set; } = [];

    // Конкретні продукти — присутні лише в листкових вузлах дерева груп
    public ICollection<ShoppingListItemDto> Items { get; set; } = [];
}

public class ShoppingListItemDto
{
    public int Id { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public float TotalWeight { get; set; }
    public decimal PricePerKg { get; set; }
    public decimal TotalPrice { get; set; }
    public bool IsPurchased { get; set; }
}