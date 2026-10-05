using Application.FoodDiary.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.FoodDiary;

public class FoodDiaryRepository(AppDbContext ctx) : IFoodDiaryRepository
{
    public async Task<FoodDiaryDay?> GetDayAsync(string userId, DateOnly date)
        => await ctx.FoodDiaryDays
            .Include(d => d.Entries).ThenInclude(e => e.Product)
            .Include(d => d.Entries).ThenInclude(e => e.Recipe)
            .FirstOrDefaultAsync(d => d.UserId == userId && d.Date == date);

    public async Task<IReadOnlyList<FoodDiaryDay>> GetDaysInRangeAsync(string userId, DateOnly from, DateOnly to)
        => await ctx.FoodDiaryDays
            .AsNoTracking()
            .Include(d => d.Entries).ThenInclude(e => e.Product)
            .Include(d => d.Entries).ThenInclude(e => e.Recipe)
            .Where(d => d.UserId == userId && d.Date >= from && d.Date <= to)
            .AsSplitQuery()
            .ToListAsync();

    public void AddDay(FoodDiaryDay day) => ctx.FoodDiaryDays.Add(day);

    public void RemoveDay(FoodDiaryDay day) => ctx.FoodDiaryDays.Remove(day);

    public async Task SaveChangesAsync() => await ctx.SaveChangesAsync();

    public async Task<CalorieGoal?> GetGoalForDateAsync(string userId, DateOnly date)
        => await ctx.CalorieGoals
            .Where(g => g.UserId == userId && g.EffectiveFrom <= date)
            .OrderByDescending(g => g.EffectiveFrom)
            .FirstOrDefaultAsync();

    public async Task<IReadOnlyList<CalorieGoal>> GetGoalsForRangeAsync(string userId, DateOnly from, DateOnly to)
    {
        var initial = await GetGoalForDateAsync(userId, from);

        var changes = await ctx.CalorieGoals
            .AsNoTracking()
            .Where(g => g.UserId == userId && g.EffectiveFrom > from && g.EffectiveFrom <= to)
            .OrderBy(g => g.EffectiveFrom)
            .ToListAsync();

        return initial is null ? changes : [initial, ..changes];
    }

    public async Task<CalorieGoal?> GetGoalByEffectiveFromAsync(string userId, DateOnly effectiveFrom)
        => await ctx.CalorieGoals
            .FirstOrDefaultAsync(g => g.UserId == userId && g.EffectiveFrom == effectiveFrom);

    public void AddGoal(CalorieGoal goal) => ctx.CalorieGoals.Add(goal);

    public async Task<IReadOnlyList<int>> GetMissingProductIdsAsync(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0) return [];
        var existing = await ctx.Products.Where(p => ids.Contains(p.Id)).Select(p => p.Id).ToListAsync();
        return ids.Except(existing).ToList();
    }

    public async Task<IReadOnlyList<int>> GetMissingRecipeIdsAsync(IReadOnlyCollection<int> ids)
    {
        if (ids.Count == 0) return [];
        var existing = await ctx.Recipes.Where(r => ids.Contains(r.Id)).Select(r => r.Id).ToListAsync();
        return ids.Except(existing).ToList();
    }
}
