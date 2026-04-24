using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupCategories.Commands;

public record DeleteLookupCategoryCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteLookupCategoryCommandHandler(
    ILookupCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ICacheService cache,
    IPublisher publisher)
    : IRequestHandler<DeleteLookupCategoryCommand, Result>
{
    public async Task<Result> Handle(DeleteLookupCategoryCommand request, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct);
        if (entity is null || entity.IsDeleted)
            return Result.NotFound($"LookupCategory {request.Id} not found.");

        entity.Delete(currentUser.UserId);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        foreach (var e in entity.DomainEvents)
            await publisher.Publish(e, ct);
        entity.ClearDomainEvents();

        await cache.RemoveByPrefixAsync("lookup_categories", ct);

        return Result.Success();
    }
}

