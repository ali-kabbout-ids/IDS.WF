using MediatR;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.DTOs;
using Rwd.WF.Application.Mappings;
using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Application.Features.LookupCategories.Queries;

public record GetAllLookupCategoriesQuery : IRequest<Result<IReadOnlyList<LookupCategoryDto>>>;

public sealed class GetAllLookupCategoriesQueryHandler(
    ILookupCategoryRepository repository,
    ICacheService cache)
    : IRequestHandler<GetAllLookupCategoriesQuery, Result<IReadOnlyList<LookupCategoryDto>>>
{
    private const string CacheKey = "lookup_categories:all";

    public async Task<Result<IReadOnlyList<LookupCategoryDto>>> Handle(GetAllLookupCategoriesQuery request, CancellationToken ct)
    {
        var cached = await cache.GetAsync<IReadOnlyList<LookupCategoryDto>>(CacheKey, ct);
        if (cached is not null)
            return Result<IReadOnlyList<LookupCategoryDto>>.Success(cached);

        var entities = await repository.GetAllAsync(ct);
        var dtos = entities
            .Where(e => !e.IsDeleted)
            .Select(e => e.ToDto())
            .ToList()
            .AsReadOnly() as IReadOnlyList<LookupCategoryDto>;

        await cache.SetAsync(CacheKey, dtos!, TimeSpan.FromMinutes(30), ct);
        return Result<IReadOnlyList<LookupCategoryDto>>.Success(dtos!);
    }
}

