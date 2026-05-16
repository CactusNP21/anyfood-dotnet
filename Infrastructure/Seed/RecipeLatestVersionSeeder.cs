using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Seed;

public static class RecipeLatestVersionSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        var recipes = await db.Recipes
            .Where(r => r.LatestVersionId == null)
            .ToListAsync();

        foreach (var recipe in recipes)
        {
            var latestVersion = await db.RecipeVersions
                .Where(rv => rv.RecipeId == recipe.Id)
                .OrderByDescending(rv => rv.VersionNumber)
                .FirstOrDefaultAsync();

            if (latestVersion is not null)
                recipe.LatestVersionId = latestVersion.Id;
        }

        await db.SaveChangesAsync();
    }
}