using Application.Products.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;

namespace Infrastructure.Products;

public class ProductPriceHistoryRepository(AppDbContext db) : IProductPriceHistoryRepository
{
    public async Task AddAsync(ProductPriceHistory history, CancellationToken ct = default)
    {
        db.ProductPriceHistories.Add(history);
        await db.SaveChangesAsync(ct);
    }
}