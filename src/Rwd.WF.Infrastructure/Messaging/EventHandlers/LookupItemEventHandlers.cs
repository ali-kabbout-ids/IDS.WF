using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;
using Rwd.WF.Domain.Events;

namespace Rwd.WF.Infrastructure.Messaging.EventHandlers;

public class LookupItemCreatedEventHandler(IBus bus, ILogger<LookupItemCreatedEventHandler> logger)
    : INotificationHandler<LookupItemCreatedEvent>
{
    public async Task Handle(LookupItemCreatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Publishing LookupItemCreated event for {ItemId} (Category {CategoryId})",
            notification.ItemId,
            notification.CategoryId);

        await bus.Publish(
            new LookupItemCreatedIntegrationEvent(notification.ItemId, notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public class LookupItemUpdatedEventHandler(IBus bus, ILogger<LookupItemUpdatedEventHandler> logger)
    : INotificationHandler<LookupItemUpdatedEvent>
{
    public async Task Handle(LookupItemUpdatedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Publishing LookupItemUpdated event for {ItemId} (Category {CategoryId})",
            notification.ItemId,
            notification.CategoryId);

        await bus.Publish(
            new LookupItemUpdatedIntegrationEvent(notification.ItemId, notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public class LookupItemDeletedEventHandler(IBus bus, ILogger<LookupItemDeletedEventHandler> logger)
    : INotificationHandler<LookupItemDeletedEvent>
{
    public async Task Handle(LookupItemDeletedEvent notification, CancellationToken ct)
    {
        logger.LogInformation(
            "Publishing LookupItemDeleted event for {ItemId} (Category {CategoryId})",
            notification.ItemId,
            notification.CategoryId);

        await bus.Publish(
            new LookupItemDeletedIntegrationEvent(notification.ItemId, notification.CategoryId, notification.Code, DateTime.UtcNow),
            ct);
    }
}

public record LookupItemCreatedIntegrationEvent(Guid ItemId, Guid CategoryId, string Code, DateTime OccurredAt);
public record LookupItemUpdatedIntegrationEvent(Guid ItemId, Guid CategoryId, string Code, DateTime OccurredAt);
public record LookupItemDeletedIntegrationEvent(Guid ItemId, Guid CategoryId, string Code, DateTime OccurredAt);

