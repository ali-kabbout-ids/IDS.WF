using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupItems.Queries.GetAll;

public record GetAllLookupItemsQuery(Guid? CategoryId = null) : IRequest<Result<IReadOnlyList<LookupItemDto>>>;

public sealed class GetAllLookupItemsQueryHandler(
    ILookupItemRepository repository,
    ICacheService cache)
    : IRequestHandler<GetAllLookupItemsQuery, Result<IReadOnlyList<LookupItemDto>>>
{
    public async Task<Result<IReadOnlyList<LookupItemDto>>> Handle(GetAllLookupItemsQuery request, CancellationToken ct)
    {
        var cacheKey = request.CategoryId is null
            ? "lookup_items:all"
            : $"lookup_items:category:{request.CategoryId}";

        var cached = await cache.GetAsync<IReadOnlyList<LookupItemDto>>(cacheKey, ct);
        if (cached is not null)
            return Result<IReadOnlyList<LookupItemDto>>.Success(cached);

        var entities = request.CategoryId is null
            ? await repository.GetAllAsync(ct)
            : await repository.GetByCategoryAsync(request.CategoryId.Value, ct);

        var dtos = entities
            .Where(e => !e.IsDeleted)
            .Select(e => e.ToDto())
            .ToList()
            .AsReadOnly() as IReadOnlyList<LookupItemDto>;

        await cache.SetAsync(cacheKey, dtos!, TimeSpan.FromMinutes(30), ct);
        return Result<IReadOnlyList<LookupItemDto>>.Success(dtos!);
    }
}

