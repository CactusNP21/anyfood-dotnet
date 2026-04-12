using MediatR;

namespace Domain.Events;

public record ProductPriceChangedEvent(int ProductId, decimal NewPrice) : INotification;