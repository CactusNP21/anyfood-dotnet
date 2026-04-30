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

    public async Task SaveRecipeAsync(int recipeId, string userId)
    { 
        ctx.SavedRecipes.Add(new SavedRecipe
        {
            UserId = userId,
            RecipeId = recipeId,
        });
        await ctx.SaveChangesAsync();
    }

    public async Task DeleteAsync(Recipe recipe)
    {
        ctx.Recipes.Remove(recipe);
        await ctx.SaveChangesAsync();
    }
    
    // ── Версіонування ────────────────────────────────────────────────────────

    public async Task<int> GetLatestVersionNumberAsync(int recipeId)
        => await ctx.RecipeVersions
            .Where(rv => rv.RecipeId == recipeId)
            .Select(rv => rv.VersionNumber)
            .DefaultIfEmpty(0)
            .MaxAsync();

    public async Task<RecipeVersion> CreateVersionAsync(RecipeVersion version)
    {
        ctx.RecipeVersions.Add(version);
        await ctx.SaveChangesAsync();
        return version;
    }

    public async Task<IReadOnlyList<RecipeVersion>> GetVersionsAsync(int recipeId)
        => await ctx.RecipeVersions
            .Where(rv => rv.RecipeId == recipeId)
            .OrderByDescending(rv => rv.VersionNumber)
            // Без інгредієнтів — для списку достатньо базових полів
            .ToListAsync();

    public async Task<RecipeVersion?> GetVersionByIdAsync(int recipeId, int versionId)
        => await ctx.RecipeVersions
            .Include(rv => rv.Ingredients)
            .ThenInclude(i => i.ProductVersion)
            .FirstOrDefaultAsync(rv => rv.RecipeId == recipeId && rv.Id == versionId);
}