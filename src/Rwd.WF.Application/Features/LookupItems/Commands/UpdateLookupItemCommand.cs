using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupItems.Commands;

public record UpdateLookupItemCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Value,
    int DisplayOrder,
    bool IsActive
) : IRequest<Result<LookupItemDto>>;

public sealed class UpdateLookupItemCommandHandler(
    ILookupItemRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ICacheService cache,
    IPublisher publisher)
    : IRequestHandler<UpdateLookupItemCommand, Result<LookupItemDto>>
{
    public async Task<Result<LookupItemDto>> Handle(UpdateLookupItemCommand request, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct);
        if (entity is null || entity.IsDeleted)
            return Result<LookupItemDto>.NotFound($"LookupItem {request.Id} not found.");

        entity.Update(
            request.NameEn,
            request.NameAr,
            request.Value,
            request.DisplayOrder,
            request.IsActive,
            currentUser.UserId);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        foreach (var e in entity.DomainEvents)
            await publisher.Publish(e, ct);
        entity.ClearDomainEvents();

        await cache.RemoveByPrefixAsync("lookup_items", ct);
        await cache.RemoveByPrefixAsync("lookup_categories", ct);

        return Result<LookupItemDto>.Success(entity.ToDto());
    }
}

