using Application.Recipes.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Recipes;

public class RecipesRepository(AppDbContext ctx): IRecipeRepository
{
    public async Task<IReadOnlyList<Recipe>> GetAllAsync()
    {
        return await ctx.Recipes.ToListAsync();
    }

    public async Task<Recipe?> GetByIdAsync(int id)
    {
        return await ctx.Recipes
            .Include(r => r.RecipeProducts)
            .ThenInclude(rp => rp.Product)
            .FirstOrDefaultAsync(rc => rc.Id == id);
    }

    public async Task<Recipe> CreateAsync(Recipe recipe)
    {
        await ctx.Recipes.AddAsync(recipe);
        await ctx.SaveChangesAsync();
        return recipe;
    }

    public async Task UpdateAsync(Recipe recipe)
    {
        ctx.Recipes.Update(recipe);
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Recipe recipe)
    {
        ctx.Recipes.Remove(recipe);
        await ctx.SaveChangesAsync();
    }
}