using Application.ShoppingList.DTO;
using Application.ShoppingList.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.ShoppingList.Service;

public class ShoppingListService(
    IEnumerable<IShoppingSourceResolver> resolvers,
    IShoppingListRepository repository) : IShoppingListService
{
    public async Task<ShoppingListDto> GenerateAsync(
        GenerateShoppingListRequest request, string userId)
    {
        // Збираємо інгредієнти з усіх джерел
        var all = new List<ShoppingIngredient>();
        foreach (var resolver in resolvers)
            all.AddRange(await resolver.ResolveAsync(request));

        // Агрегуємо по ProductVersionId
        var aggregated = all
            .GroupBy(i => i.ProductId)
            .Select(g => new ShoppingListItem
            {
                ProductId = g.Key,
                TotalWeight = g.Sum(i => i.Weight),
            })
            .ToList();

        var shoppingList = new global::Domain.Entities.ShoppingList
        {
            Name = request.Name,
            UserId = userId,
            Items = aggregated,
        };

        var created = await repository.CreateAsync(shoppingList);
        return ToDto(created);
    }

    public async Task<IReadOnlyList<ShoppingListDto>> GetByUserAsync(string userId)
    {
        var lists = await repository.GetByUserAsync(userId);
        return lists.Select(ToDto).ToList();  // was: lists.Adapt<IReadOnlyList<ShoppingListDto>>()
    }

    public async Task<ShoppingListDto> GetByIdAsync(int id)
    {
        throw new NotImplementedException();
    }

    public async Task TogglePurchasedAsync(int shoppingListId, int itemId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(int id)
    {
        throw new NotImplementedException();
    }
    
    private static ShoppingListDto ToDto(Domain.Entities.ShoppingList list) => new()
    {
        Id = list.Id,
        Name = list.Name,
        CreatedAt = list.CreatedAt,
        Items = list.Items.Select(i => new ShoppingListItemDto
        {
            Id = i.Id,
            ProductName = i.Product.Name,
            ImageUrl = i.Product.ImageUrl,
            TotalWeight = i.TotalWeight,
            PricePerKg = i.Product.Price,
            TotalPrice = i.Product.Price * (decimal)(i.TotalWeight / 1000f),
            IsPurchased = i.IsPurchased,
        }).ToList(),
        TotalPrice = list.Items.Sum(i =>
            i.Product.Price * (decimal)(i.TotalWeight / 1000f)),
    };
}