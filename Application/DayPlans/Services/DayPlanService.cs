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
    // Application/DayPlans/Services/DayPlanService.cs
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
                });
            }
            else if (e.RecipeId is not null)
            {
                var version = await recipeRepository.GetLatestVersionAsync(e.RecipeId.Value)  // треба додати цей метод
                              ?? throw new KeyNotFoundException($"Рецепт з id={e.RecipeId} не має версій.");

                entries.Add(new DayPlanEntry
                {
                    RecipeVersionId = version.Id,
                    Weight = e.Weight,
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
        var dayPlan = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("План дня не знайдено.");
        return dayPlan.Adapt<DayPlanDto>();
    }

    public async Task<IReadOnlyList<DayPlanDto>> GetByUserAsync(string userId)
    {
        var plans = await repository.GetByUserAsync(userId);
        return plans.Select(p => p.Adapt<DayPlanDto>()).ToList();
    }

    public async Task DeleteAsync(int id)
    {
        var dayPlan = await repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("План дня не знайдено.");
        await repository.DeleteAsync(dayPlan);
    }

    // ── Private ──────────────────────────────────────────────────────────────

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