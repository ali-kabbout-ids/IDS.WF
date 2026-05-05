using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupCategories.Commands;

public record UpdateLookupCategoryCommand(
    Guid Id,
    string NameEn,
    string NameAr,
    string? Description,
    int DisplayOrder,
    bool IsActive
) : IRequest<Result<LookupCategoryDto>>;

public sealed class UpdateLookupCategoryCommandHandler(
    ILookupCategoryRepository repository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUser,
    ICacheService cache,
    IPublisher publisher)
    : IRequestHandler<UpdateLookupCategoryCommand, Result<LookupCategoryDto>>
{
    public async Task<Result<LookupCategoryDto>> Handle(UpdateLookupCategoryCommand request, CancellationToken ct)
    {
        var entity = await repository.GetByIdAsync(request.Id, ct);
        if (entity is null || entity.IsDeleted)
            return Result<LookupCategoryDto>.NotFound($"LookupCategory {request.Id} not found.");

        entity.Update(
            request.NameEn,
            request.NameAr,
            request.Description,
            request.DisplayOrder,
            request.IsActive,
            currentUser.UserId);

        repository.Update(entity);
        await unitOfWork.SaveChangesAsync(ct);

        foreach (var e in entity.DomainEvents)
            await publisher.Publish(e, ct);
        entity.ClearDomainEvents();

        await cache.RemoveByPrefixAsync("lookup_categories", ct);

        return Result<LookupCategoryDto>.Success(entity.ToDto());
    }
}

