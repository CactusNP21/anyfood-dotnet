// Infrastructure/DayPlans/DayPlanRepository.cs
using Application.DayPlans.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DayPlans;

public class DayPlanRepository(AppDbContext ctx) : IDayPlanRepository
{
    public async Task<DayPlan?> GetByIdAsync(int id)
        => await ctx.DayPlans
            .Include(d => d.Entries)
            .FirstOrDefaultAsync(d => d.Id == id);

    public async Task<DayPlan?> GetByUserAndDateAsync(string userId, DateTime date)
    {
        throw new NotImplementedException();
    }
    
    public async Task<IReadOnlyList<DayPlan>> GetByUserAsync(string userId)
        => await ctx.DayPlans
            .Include(d => d.Entries)
            .Where(d => d.UserId == userId)
            .ToListAsync();

    public async Task<DayPlan> CreateAsync(DayPlan dayPlan)
    {
        ctx.DayPlans.Add(dayPlan);
        await ctx.SaveChangesAsync();
        return dayPlan;
    }

    public async Task DeleteAsync(DayPlan dayPlan)
    {
        ctx.DayPlans.Remove(dayPlan);
        await ctx.SaveChangesAsync();
    }
    
    public async Task<DayPlan?> GetByIdWithDetailsAsync(int id)
        => await ctx.DayPlans
            .Include(d => d.Entries)
            .ThenInclude(e => e.ProductVersion)
            .Include(d => d.Entries)
            .ThenInclude(e => e.RecipeVersion)
            .ThenInclude(rv => rv!.Ingredients)
            .ThenInclude(i => i.ProductVersion)
            .FirstOrDefaultAsync(d => d.Id == id);
}