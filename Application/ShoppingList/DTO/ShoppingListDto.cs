public class ShoppingListDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<ShoppingListItemDto> Items { get; set; } = [];
    public decimal TotalPrice { get; set; }
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