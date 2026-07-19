// Application/Fridge/Services/FridgeService.cs
using Application.Fridge.DTOs;
using Application.Fridge.Interfaces;
using Application.Recipes.Interfaces;
using Application.ShoppingList.Interfaces;

namespace Application.Fridge.Services;

public class FridgeService(
    IFridgeRepository fridgeRepository,
    IShoppingListRepository shoppingListRepository,
    IRecipeRepository recipeRepository) : IFridgeService
{
    public async Task<FridgeDto> GetMyFridgeAsync(string userId)
    {
        var fridge = await fridgeRepository.GetOrCreateAsync(userId);
        return new FridgeDto
        {
            Id = fridge.Id,
            Items = fridge.Items.Select(i => new FridgeItemDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.Product.Name,
                ImageUrl = i.Product.ImageUrl,
                Weight = i.Weight,
            }).ToList(),
        };
    }

    public async Task AddFromShoppingListAsync(int shoppingListId, string userId)
    {
        var list = await shoppingListRepository.GetByIdAsync(shoppingListId)
            ?? throw new KeyNotFoundException("Список покупок не знайдено.");

        if (list.UserId != userId)
            throw new UnauthorizedAccessException();

        var fridge = await fridgeRepository.GetOrCreateAsync(userId);

        // ShoppingListItem.TotalWeight - це вага, необхідна для рецептів (їстивна, EP).
        // Купуємо ж продукт цілим (AP) - тому ділимо на EdiblePortionFactor,
        // щоб у холодильнику опинилась реальна закуплена вага (з шкіркою тощо).
        var stockToAdd = list.Items.Select(i => (
            ProductId: i.ProductId,
            Weight: i.TotalWeight / i.Product.EdiblePortionFactor
        )).ToList();

        await fridgeRepository.AddStockAsync(fridge.Id, stockToAdd);
    }

    public async Task<ConsumeRecipeResultDto> ConsumeForRecipeAsync(ConsumeRecipeRequest request, string userId)
    {
        var version = await recipeRepository.GetByIdAsync(request.RecipeId)
            ?? throw new KeyNotFoundException("Рецепт не має версій.");

        var fridge = await fridgeRepository.GetOrCreateAsync(userId);

        // Інгредієнти рецепту - це їстивна (EP) вага.
        // Переводимо назад у "закуплену" (AP) вагу, бо саме в такому вигляді
        // продукт зберігається в холодильнику (з шкіркою/лушпинням).
        var versionWithIngredients = await recipeRepository.GetByIdAsync(version.Id)
            ?? throw new KeyNotFoundException();

        var toSubtract = versionWithIngredients.RecipeProducts.Select(ing => (
            ProductId: ing.ProductId,
            Weight: ing.Weight * request.ServingsMultiplier / ing.Product.EdiblePortionFactor
        )).ToList();

        var shortfalls = await fridgeRepository.SubtractStockAsync(fridge.Id, toSubtract);

        return new ConsumeRecipeResultDto
        {
            Shortfalls = shortfalls.Select(kv => new FridgeShortfallDto
            {
                ProductId = kv.Key,
                ProductName = versionWithIngredients.RecipeProducts
                    .First(i => i.ProductId == kv.Key).Product.Name,
                MissingWeight = kv.Value,
            }).ToList(),
        };
    }
}