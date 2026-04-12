using Application.Products.Interfaces;
using Domain.Entities;
using Domain.Events;
using MediatR;

namespace Application.Products.EventHandlers;

public class ProductPriceChangedHandler(IProductPriceHistoryRepository priceHistoryRepository)
    : INotificationHandler<ProductPriceChangedEvent>
{
    public async Task Handle(ProductPriceChangedEvent e, CancellationToken ct)
    {
        await priceHistoryRepository.AddAsync(new ProductPriceHistory
        {
            ProductId = e.ProductId,
            Price = e.NewPrice,
        }, ct);
    }
}