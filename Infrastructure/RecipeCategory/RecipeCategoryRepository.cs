using Application.RecipeCategories.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.RecipeCategory;

public class RecipeCategoryRepository(AppDbContext ctx):IRecipeCategoryRepository
{
    public async Task<IReadOnlyList<Domain.Entities.RecipeCategory>> GetAllAsync()
    {
        return await ctx.RecipeCategories.ToListAsync();
    }

    public async Task<Domain.Entities.RecipeCategory?> GetByIdAsync(int id)
    {
        return await ctx.RecipeCategories.FirstOrDefaultAsync(rc => rc.Id == id);
    }

    public Task<Domain.Entities.RecipeCategory?> GetByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public async Task<Domain.Entities.RecipeCategory> CreateAsync(Domain.Entities.RecipeCategory category)
    {
        await ctx.RecipeCategories.AddAsync(category);
        await ctx.SaveChangesAsync();
        return category;
    }

    public async Task UpdateAsync(Domain.Entities.RecipeCategory category)
    {
        ctx.RecipeCategories.Update(category);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Domain.Entities.RecipeCategory category)
    {
        ctx.RecipeCategories.Remove(category);
        await ctx.SaveChangesAsync();
    }
}