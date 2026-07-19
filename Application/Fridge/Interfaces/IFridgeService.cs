// Application/Fridge/Interfaces/IFridgeService.cs
using Application.Fridge.DTOs;

namespace Application.Fridge.Interfaces;

public interface IFridgeService
{
    Task<FridgeDto> GetMyFridgeAsync(string userId);
    Task AddFromShoppingListAsync(int shoppingListId, string userId);
    Task<ConsumeRecipeResultDto> ConsumeForRecipeAsync(ConsumeRecipeRequest request, string userId);
}