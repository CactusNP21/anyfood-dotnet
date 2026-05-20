// Application/ShoppingLists/Resolvers/DayPlanSourceResolver.cs

using Application.DayPlans.Interfaces;
using Application.ShoppingList.DTO;

public class DayPlanSourceResolver(IDayPlanRepository dayPlanRepository) : IShoppingSourceResolver
{
    public async Task<IEnumerable<ShoppingIngredient>> ResolveAsync(
        GenerateShoppingListRequest request, CancellationToken ct = default)
    {
        var result = new List<ShoppingIngredient>();

        foreach (var planId in request.DayPlanIds)
        {
            var plan = await dayPlanRepository.GetByIdWithDetailsAsync(planId)
                       ?? throw new KeyNotFoundException($"DayPlan id={planId} не знайдено.");

            foreach (var entry in plan.Entries)
            {
                if (entry.ProductId is not null)
                {
                    result.Add(new ShoppingIngredient(
                        entry.ProductId.Value, 
                        entry.Product,
                        entry.Weight));
                }

                if (entry.RecipeVersion is not null)
                {
                    var total = entry.RecipeVersion.Ingredients.Sum(i => i.Weight);
                    foreach (var ingredient in entry.RecipeVersion.Ingredients)
                    {
                        var ratio = total > 0 ? ingredient.Weight / total : 0;
                        result.Add(new ShoppingIngredient(
                            ingredient.ProductId,
                            ingredient.Product,
                            entry.Weight * ratio));
                    }
                }
            }
        }

        return result;
    }
}