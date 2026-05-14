// Domain/Entities/ShoppingList.cs

using Domain.Entities;

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

    // Посилання на конкретну версію продукту (snapshot ціни/назви)
    public int ProductVersionId { get; set; }
    public ProductVersion ProductVersion { get; set; } = null!;

    public float TotalWeight { get; set; } // грами
    public bool IsPurchased { get; set; }
}