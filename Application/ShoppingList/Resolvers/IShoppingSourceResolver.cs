// Application/ShoppingLists/Resolvers/IShoppingSourceResolver.cs

using Application.ShoppingList.DTO;
using Domain.Entities;

public interface IShoppingSourceResolver
{
    Task<IEnumerable<ShoppingIngredient>> ResolveAsync(
        GenerateShoppingListRequest request, CancellationToken ct = default);
}

public record ShoppingIngredient(int ProductVersionId, ProductVersion Version, float Weight);