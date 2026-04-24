using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Rwd.WF.Domain.Events;

namespace Rwd.WF.Infrastructure.Messaging.EventHandlers;

public class LookupCategoryCreatedEventHandler(IBus bus, ILogger<LookupCategoryCreatedEventHandler> logger)
    : INotificationHandler<LookupCategoryCreatedEvent>
{
    public async Task Handle(LookupCategoryCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Publishing LookupCategoryCreated event for {CategoryId}", notification.CategoryId);
        await bus.Publish(
            new LookupCategoryCreatedIntegrationEvent(notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public class LookupCategoryUpdatedEventHandler(IBus bus, ILogger<LookupCategoryUpdatedEventHandler> logger)
    : INotificationHandler<LookupCategoryUpdatedEvent>
{
    public async Task Handle(LookupCategoryUpdatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Publishing LookupCategoryUpdated event for {CategoryId}", notification.CategoryId);
        await bus.Publish(
            new LookupCategoryUpdatedIntegrationEvent(notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public class LookupCategoryDeletedEventHandler(IBus bus, ILogger<LookupCategoryDeletedEventHandler> logger)
    : INotificationHandler<LookupCategoryDeletedEvent>
{
    public async Task Handle(LookupCategoryDeletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation("Publishing LookupCategoryDeleted event for {CategoryId}", notification.CategoryId);
        await bus.Publish(
            new LookupCategoryDeletedIntegrationEvent(notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public record LookupCategoryCreatedIntegrationEvent(Guid CategoryId, string Code, DateTime OccurredAt);
public record LookupCategoryUpdatedIntegrationEvent(Guid CategoryId, string Code, DateTime OccurredAt);
public record LookupCategoryDeletedIntegrationEvent(Guid CategoryId, string Code, DateTime OccurredAt);

