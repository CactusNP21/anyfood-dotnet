// Application/ShoppingLists/Interfaces/IShoppingListRepository.cs

using Domain.Entities;

namespace Application.ShoppingList.Interfaces;

public interface IShoppingListRepository
{
    Task<global::Domain.Entities.ShoppingList> CreateAsync(global::Domain.Entities.ShoppingList shoppingList);
    Task<global::Domain.Entities.ShoppingList?> GetByIdAsync(int id);
    Task<IReadOnlyList<global::Domain.Entities.ShoppingList>> GetByUserAsync(string userId);
    Task<ShoppingListItem?> GetItemByIdAsync(int itemId);
    Task UpdateItemAsync(ShoppingListItem item);
    Task DeleteAsync(global::Domain.Entities.ShoppingList shoppingList);
}