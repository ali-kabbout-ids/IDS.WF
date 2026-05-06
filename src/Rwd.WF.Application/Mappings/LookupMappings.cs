using Rwd.WF.Application.DTOs;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Application.Mappings;

public static class LookupMappings
{
    public static LookupCategoryDto ToDto(this LookupCategory entity) => new(
        entity.Id,
        entity.Code,
        entity.NameEn,
        entity.NameAr,
        entity.Description,
        entity.IsActive,
        entity.DisplayOrder,
        entity.CreatedAt,
        entity.CreatedBy);

    public static LookupItemDto ToDto(this LookupItem entity) => new(
        entity.Id,
        entity.CategoryId,
        entity.Code,
        entity.NameEn,
        entity.NameAr,
        entity.Value,
        entity.IsActive,
        entity.DisplayOrder);

    public static LookupCategoryWithItemsDto ToDtoWithItems(this LookupCategory entity) => new(
        entity.Id,
        entity.Code,
        entity.NameEn,
        entity.NameAr,
        entity.Description,
        entity.IsActive,
        entity.DisplayOrder,
        entity.Items
            .Where(i => !i.IsDeleted)
            .Select(i => i.ToDto())
            .ToList()
            .AsReadOnly());
}

