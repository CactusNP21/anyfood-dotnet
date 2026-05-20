// Application/ShoppingLists/Resolvers/RecipeSourceResolver.cs

using Application.Recipes.Interfaces;
using Application.ShoppingList.DTO;

public class RecipeSourceResolver(IRecipeRepository recipeRepository) : IShoppingSourceResolver
{
    public async Task<IEnumerable<ShoppingIngredient>> ResolveAsync(
        GenerateShoppingListRequest request, CancellationToken ct = default)
    {
        var result = new List<ShoppingIngredient>();

        foreach (var source in request.Recipes)
        {
            var version = await recipeRepository.GetVersionByIdAsync(source.RecipeVersionId)
                          ?? throw new KeyNotFoundException($"Рецепт id={source.RecipeVersionId} не має версій.");

            var totalWeight = version.Ingredients.Sum(i => i.Weight);

            foreach (var ingredient in version.Ingredients)
            {
                var ratio = totalWeight > 0 ? ingredient.Weight / totalWeight : 0;
                var weight = ratio * source.Weight;

                result.Add(new ShoppingIngredient(
                    ingredient.ProductId,
                    ingredient.Product,
                    weight));
            }
        }

        return result;
    }
}