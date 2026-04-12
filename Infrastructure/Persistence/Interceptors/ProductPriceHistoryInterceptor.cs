using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Infrastructure.Persistence.Interceptors;

public class ProductPriceHistoryInterceptor: SaveChangesInterceptor
{
    private bool _isSavingHistory; 
    private List<Product> _newProducts = [];

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        if (eventData.Context is null)
            return base.SavingChangesAsync(eventData, result, ct);

        // Запам'ятовуємо нові продукти — Id ще немає
        _newProducts = eventData.Context.ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => e.Entity)
            .ToList();

        // Змінені продукти — Id вже є, додаємо одразу
        var priceChanges = eventData.Context.ChangeTracker.Entries<Product>()
            .Where(e => e.State == EntityState.Modified)
            .Where(e => (decimal)e.OriginalValues[nameof(Product.Price)]! != e.Entity.Price)
            .Select(e => new ProductPriceHistory
            {
                ProductId = e.Entity.Id,
                Price = e.Entity.Price,
            })
            .ToList();

        eventData.Context.Set<ProductPriceHistory>().AddRange(priceChanges);

        return base.SavingChangesAsync(eventData, result, ct);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken ct = default)
    {
        // Після збереження — Id вже заповнений
        if (!_isSavingHistory && _newProducts.Count > 0 && eventData.Context is not null)
        {
            var histories = _newProducts
                .Select(p => new ProductPriceHistory
                {
                    ProductId = p.Id, // тепер є Id
                    Price = p.Price,
                })
                .ToList();

            _newProducts = [];
            _isSavingHistory = true;
            eventData.Context.Set<ProductPriceHistory>().AddRange(histories);
            await eventData.Context.SaveChangesAsync(ct);
            _isSavingHistory = false;
        }

        return await base.SavedChangesAsync(eventData, result, ct);
    }
}