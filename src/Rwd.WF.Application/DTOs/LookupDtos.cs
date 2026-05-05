namespace Rwd.WF.Application.DTOs;

public record LookupCategoryDto(
    Guid Id,
    string Code,
    string NameEn,
    string NameAr,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    DateTime CreatedAt,
    string CreatedBy
);

public record LookupCategoryWithItemsDto(
    Guid Id,
    string Code,
    string NameEn,
    string NameAr,
    string? Description,
    bool IsActive,
    int DisplayOrder,
    IReadOnlyCollection<LookupItemDto> Items
);

public record LookupItemDto(
    Guid Id,
    Guid CategoryId,
    string Code,
    string NameEn,
    string NameAr,
    string? Value,
    bool IsActive,
    int DisplayOrder
);

