using Domain.Entities;

namespace Application.Products.Interfaces;

public interface IProductPriceHistoryRepository
{
    Task AddAsync(ProductPriceHistory history, CancellationToken ct = default);
}