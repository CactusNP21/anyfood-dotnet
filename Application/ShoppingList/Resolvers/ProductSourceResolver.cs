// Application/ShoppingLists/Resolvers/ProductSourceResolver.cs

using Application.Products.Interfaces;
using Application.ShoppingList.DTO;

public class ProductSourceResolver(IProductRepository productRepository) : IShoppingSourceResolver
{
    public async Task<IEnumerable<ShoppingIngredient>> ResolveAsync(
        GenerateShoppingListRequest request, CancellationToken ct = default)
    {
        var result = new List<ShoppingIngredient>();

        foreach (var source in request.Products)
        {
            var version = await productRepository.GetProductVersionByIdAsync(source.ProductVersionId)
                          ?? throw new KeyNotFoundException($"Продукт id={source.ProductVersionId} не має версій.");

            result.Add(new ShoppingIngredient(version.Id, version, source.Weight));
        }

        return result;
    }
}