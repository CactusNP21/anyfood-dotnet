// Application/Fridge/Interfaces/IFridgeRepository.cs
using Domain.Entities;

namespace Application.Fridge.Interfaces;

public interface IFridgeRepository
{
    Task<Domain.Entities.Fridge?> GetByUserIdAsync(string userId);
    Task<Domain.Entities.Fridge> GetOrCreateAsync(string userId);

    // Додає вагу до існуючих items, або створює новий item якщо продукту ще нема
    Task AddStockAsync(int fridgeId, IReadOnlyList<(int ProductId, float Weight)> items);

    // Віднімає вагу; якщо не вистачає - забирає скільки є (до 0), решту повертає як shortfall
    Task<IReadOnlyDictionary<int, float>> SubtractStockAsync(
        int fridgeId, IReadOnlyList<(int ProductId, float Weight)> items);
}