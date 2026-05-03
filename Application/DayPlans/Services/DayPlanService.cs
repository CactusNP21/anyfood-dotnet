// Application/DayPlans/Services/DayPlanService.cs

using System.Security.Claims;
using Application.DayPlans.DTOs;
using Application.DayPlans.Interfaces;
using Domain.Entities;
using Mapster;

namespace Application.DayPlans.Services;

public class DayPlanService(IDayPlanRepository repository) : IDayPlanService
{
    public async Task<DayPlanDto> CreateAsync(CreateDayPlanRequest request, string userId)
    {
       
        ValidateEntries(request.Entries);
        
        var dayPlan = new DayPlan
        {
            Name = request.Name,
            UserId = userId,
            Entries = request.Entries.Select(e => new DayPlanEntry
            {
                RecipeId = e.RecipeId,
                ProductId = e.ProductId,
                Weight = e.Weight,
            }).ToList(),
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

    private static void ValidateEntries(ICollection<DayPlanEntryDto> entries)
    {
        if (entries.Count == 0)
            throw new InvalidOperationException("План дня не може бути порожнім.");

        foreach (var entry in entries)
        {
            if (entry.RecipeId is null && entry.ProductId is null)
                throw new InvalidOperationException(
                    "Кожен запис повинен містити рецепт або продукт.");

            if (entry.RecipeId is not null && entry.ProductId is not null)
                throw new InvalidOperationException(
                    "Запис не може одночасно містити рецепт і продукт.");

            if (entry.Weight <= 0)
                throw new InvalidOperationException("Вага повинна бути більше 0.");
        }
    }
}