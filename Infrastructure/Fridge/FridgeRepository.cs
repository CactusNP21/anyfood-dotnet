// Infrastructure/Fridge/FridgeRepository.cs
using Application.Fridge.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Fridge;

public class FridgeRepository(AppDbContext ctx) : IFridgeRepository
{
    public async Task<Domain.Entities.Fridge?> GetByUserIdAsync(string userId)
        => await ctx.Fridges
            .Include(f => f.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(f => f.UserId == userId);

    public async Task<Domain.Entities.Fridge> GetOrCreateAsync(string userId)
    {
        var fridge = await GetByUserIdAsync(userId);
        if (fridge is not null) return fridge;

        fridge = new Domain.Entities.Fridge { UserId = userId };
        ctx.Fridges.Add(fridge);
        await ctx.SaveChangesAsync();
        return fridge;
    }

    public async Task AddStockAsync(int fridgeId, IReadOnlyList<(int ProductId, float Weight)> items)
    {
        var productIds = items.Select(i => i.ProductId).ToList();

        var existing = await ctx.FridgeItems
            .Where(fi => fi.FridgeId == fridgeId && productIds.Contains(fi.ProductId))
            .ToDictionaryAsync(fi => fi.ProductId);

        foreach (var (productId, weight) in items)
        {
            if (existing.TryGetValue(productId, out var item))
            {
                item.Weight += weight;
            }
            else
            {
                ctx.FridgeItems.Add(new FridgeItem
                {
                    FridgeId = fridgeId,
                    ProductId = productId,
                    Weight = weight,
                });
            }
        }

        await ctx.SaveChangesAsync();
    }

    public async Task<IReadOnlyDictionary<int, float>> SubtractStockAsync(
        int fridgeId, IReadOnlyList<(int ProductId, float Weight)> items)
    {
        var productIds = items.Select(i => i.ProductId).ToList();

        var existing = await ctx.FridgeItems
            .Where(fi => fi.FridgeId == fridgeId && productIds.Contains(fi.ProductId))
            .ToDictionaryAsync(fi => fi.ProductId);

        var shortfalls = new Dictionary<int, float>();

        foreach (var (productId, weight) in items)
        {
            if (!existing.TryGetValue(productId, out var item))
            {
                // продукту взагалі немає в холодильнику - все "не вистачило"
                shortfalls[productId] = weight;
                continue;
            }

            if (item.Weight >= weight)
            {
                item.Weight -= weight;
            }
            else
            {
                shortfalls[productId] = weight - item.Weight;
                item.Weight = 0;
            }
        }

        await ctx.SaveChangesAsync();
        return shortfalls;
    }
}