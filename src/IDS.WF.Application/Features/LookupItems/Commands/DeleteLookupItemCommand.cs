using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupItems.Commands;

public record DeleteLookupItemCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteLookupItemCommandHandler(
    ILookupItemRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ICacheService cache,
    IPublisher publisher)
    : IRequestHandler<DeleteLookupItemCommand, Result>
{
    public async Task<Result> Handle(DeleteLookupItemCommand request, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct);
        if (entity is null || entity.IsDeleted)
            return Result.NotFound($"LookupItem {request.Id} not found.");

        entity.Delete(currentUser.UserId);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        foreach (var e in entity.DomainEvents)
            await publisher.Publish(e, ct);
        entity.ClearDomainEvents();

        await cache.RemoveByPrefixAsync("lookup_items", ct);
        await cache.RemoveByPrefixAsync("lookup_categories", ct);

        return Result.Success();
    }
}

