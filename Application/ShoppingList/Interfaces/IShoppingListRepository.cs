// Application/ShoppingLists/Interfaces/IShoppingListRepository.cs
namespace Application.ShoppingList.Interfaces;

public interface IShoppingListRepository
{
    Task<global::ShoppingList> CreateAsync(global::ShoppingList shoppingList);
    Task<global::ShoppingList?> GetByIdAsync(int id);
    Task<IReadOnlyList<global::ShoppingList>> GetByUserAsync(string userId);
    Task<ShoppingListItem?> GetItemByIdAsync(int itemId);
    Task UpdateItemAsync(ShoppingListItem item);
    Task DeleteAsync(global::ShoppingList shoppingList);
}