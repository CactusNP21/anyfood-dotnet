// Application/ShoppingLists/Resolvers/ProductSourceResolver.cs

using Application.Products.Interfaces;
using Application.ShoppingList.DTO;

namespace Application.ShoppingList.Resolvers;

public class ProductSourceResolver(IProductRepository productRepository) : IShoppingSourceResolver
{
    public async Task<IEnumerable<ShoppingIngredient>> ResolveAsync(
        GenerateShoppingListRequest request, CancellationToken ct = default)
    {
        var result = new List<ShoppingIngredient>();

        foreach (var source in request.Products)
        {
            var product = await productRepository.GetByIdAsync(source.ProductId)
                          ?? throw new KeyNotFoundException($"Продукт id={source.ProductId} не існує");

            result.Add(new ShoppingIngredient(product.Id, product, source.Weight));
        }

        return result;
    }
}