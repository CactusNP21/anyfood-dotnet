// Application/ShoppingLists/Interfaces/IShoppingListService.cs

using Application.ShoppingList.DTO;

namespace Application.ShoppingList.Interfaces;

public interface IShoppingListService
{
    Task<ShoppingListDto> GenerateAsync(GenerateShoppingListRequest request, string userId);
    Task<IReadOnlyList<ShoppingListDto>> GetByUserAsync(string userId);
    Task<ShoppingListDto> GetByIdAsync(int id);
    Task TogglePurchasedAsync(int shoppingListId, int itemId);
    Task DeleteAsync(int id);
}