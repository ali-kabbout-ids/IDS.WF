using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupItems.Queries;

public record GetLookupItemByIdQuery(Guid Id) : IRequest<Result<LookupItemDto>>;

public sealed class GetLookupItemByIdQueryHandler(
    ILookupItemRepository repository,
    ICacheService cache)
    : IRequestHandler<GetLookupItemByIdQuery, Result<LookupItemDto>>
{
    public async Task<Result<LookupItemDto>> Handle(GetLookupItemByIdQuery request, CancellationToken ct)
    {
        var cacheKey = $"lookup_items:{request.Id}";
        var cached = await cache.GetAsync<LookupItemDto>(cacheKey, ct);
        if (cached is not null)
            return Result<LookupItemDto>.Success(cached);

        var entity = await repository.GetByIdAsync(request.Id, ct);
        if (entity is null || entity.IsDeleted)
            return Result<LookupItemDto>.NotFound();

        var dto = entity.ToDto();
        await cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(30), ct);
        return Result<LookupItemDto>.Success(dto);
    }
}

