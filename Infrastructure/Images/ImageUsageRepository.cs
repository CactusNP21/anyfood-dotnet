using Application.Images.Interfaces;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Images;

public class ImageUsageRepository(AppDbContext ctx) : IImageUsageRepository
{
    // Contains, а не StartsWith — на випадок, якщо десь збережено абсолютний URL
    public async Task<bool> IsInUseAsync(string hash)
    {
        var segment = $"/images/{hash}/";

        return await ctx.Recipes.AnyAsync(r => r.ImageUrl.Contains(segment))
            || await ctx.RecipeSteps.AnyAsync(s => s.ImageUrl != null && s.ImageUrl.Contains(segment))
            || await ctx.Products.AnyAsync(p => p.ImageUrl != null && p.ImageUrl.Contains(segment))
            || await ctx.Users.AnyAsync(u => u.AvatarUrl != null && u.AvatarUrl.Contains(segment));
    }
}
