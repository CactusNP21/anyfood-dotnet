using Application.DayPlans.DTOs;
using Application.DayPlans.Interfaces;
using Application.Products.Interfaces;
using Application.Recipes.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.DayPlans.Services;

public class DayPlanService(
    IDayPlanRepository repository,
    IProductRepository productRepository,
    IRecipeRepository recipeRepository) : IDayPlanService
{
    public async Task<DayPlanDto> CreateAsync(CreateDayPlanRequest request, string userId)
    {
        ValidateEntries(request.Entries);

        var entries = new List<DayPlanEntry>();

        foreach (var e in request.Entries)
        {
            if (e.ProductId is not null)
            {
                entries.Add(new DayPlanEntry
                {
                    ProductId = e.ProductId,
                    Weight = e.Weight,
                    Time = e.Time,
                });
            }
            else if (e.RecipeId is not null)
            {
                var version = await recipeRepository.GetByIdAsync(e.RecipeId.Value)  // треба додати цей метод
                              ?? throw new KeyNotFoundException($"Рецепт з id={e.RecipeId} не має версій.");

                entries.Add(new DayPlanEntry
                {
                    RecipeId = version.Id,
                    Weight = e.Weight,
                    Time = e.Time,
                });
            }
        }

        var dayPlan = new DayPlan
        {
            Name = request.Name,
            UserId = userId,
            Entries = entries,
        };

        var created = await repository.CreateAsync(dayPlan);
        return created.Adapt<DayPlanDto>();
    }

    public async Task<DayPlanDto> GetByIdAsync(int id)
    {
        var dayPlan = await repository.GetByIdWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("План дня не знайдено.");
        return ToDayPlanDto(dayPlan);
    }

    public async Task<IReadOnlyList<UserDayPlanShort>> GetByUserAsync(string userId)
    {
        var plans = await repository.GetByUserAsync(userId);
        return plans.Select(dp => new UserDayPlanShort{ Name = dp.Name, Id = dp.Id }).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var dayPlan = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("План дня не знайдено.");
        await repository.DeleteAsync(dayPlan);
    }

    // ── Private ──────────────────────────────────────────────────────────────

    // Application/DayPlans/Services/DayPlanService.cs
    private static DayPlanDto ToDayPlanDto(DayPlan dayPlan)
    {
        float totalCalories = 0, totalProtein = 0, totalFat = 0, totalCarbs = 0, totalPrice = 0;

        foreach (var entry in dayPlan.Entries)
        {
            var ratio = entry.Weight / 100f;

            if (entry.Product is not null)
            {
                totalCalories += (float)entry.Product.Calories * ratio;
                totalProtein  += (float)entry.Product.Protein  * ratio;
                totalFat      += (float)entry.Product.Fat      * ratio;
                totalCarbs    += (float)entry.Product.Carbs    * ratio;
                totalPrice    += (float)entry.Product.Price    * ratio;
            }
            else if (entry.Recipe is not null)
            {
                totalCalories += entry.Recipe.Calories * ratio;
                totalProtein  += entry.Recipe.Protein  * ratio;
                totalFat      += entry.Recipe.Fat      * ratio;
                totalCarbs    += entry.Recipe.Carbs    * ratio;
                totalPrice    += entry.Recipe.Price    * ratio;
            }
            // якщо обидва null - запис некоректний (не мало б статись через валідацію),
            // просто пропускаємо
        }
        
        return new DayPlanDto
        {
            Id = dayPlan.Id,
            Name = dayPlan.Name,
            UserId = dayPlan.UserId,
            Entries = dayPlan.Entries.Select(e => new DayPlanEntryResultDto
            {
                Id = e.Id,
                Weight = e.Weight,
                RecipeId = e.RecipeId,
                ProductId = e.ProductId,
                Time = e.Time,
                ImageUrl = e.Recipe != null ? e.Recipe.ImageUrl : e.Product?.ImageUrl,
                Name = e.ProductId is not null ? e.Product!.Name : e.Recipe!.Name,
                Product = e.Product,
                Recipe = e.Recipe,
            }).ToList(),
            TotalCalories = totalCalories,
            TotalProtein = totalProtein,
            TotalFats = totalFat,
            TotalCarbs = totalCarbs,
            TotalPrice = totalPrice,
        };
    }
    
    private static void ValidateEntries(ICollection<DayPlanEntryDto> entries)
    {
        if (entries.Count == 0)
            throw new InvalidOperationException("План дня не може бути порожнім.");

        foreach (var entry in entries)
        {
            if (entry.RecipeId is null && entry.ProductId is null)
                throw new InvalidOperationException(
                    "Кожен запис повинен містити версію рецепту або версію продукту.");

            if (entry.RecipeId is not null && entry.ProductId is not null)
                throw new InvalidOperationException(
                    "Запис не може одночасно містити версію рецепту і версію продукту.");

            if (entry.Weight <= 0)
                throw new InvalidOperationException("Вага повинна бути більше 0.");
        }
    }
}