// Domain/Entities/ShoppingList.cs

namespace Domain.Entities;

public class ShoppingList
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string UserId { get; set; } = null!;
    public User User { get; set; } = null!;

    public ICollection<ShoppingListItem> Items { get; set; } = [];
}

public class ShoppingListItem
{
    public int Id { get; set; }

    public int ShoppingListId { get; set; }
    public ShoppingList ShoppingList { get; set; } = null!;

    public required int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public float TotalWeight { get; set; } // грами
    public bool IsPurchased { get; set; }
}