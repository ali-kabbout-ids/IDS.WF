using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Events;

namespace Rwd.WF.Domain.Entities;

public sealed class LookupItem : BaseEntity
{
    public Guid CategoryId { get; private set; }
    public LookupCategory Category { get; private set; } = null!;
    public string Code { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string? Value { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private LookupItem() { } // EF

    public static LookupItem Create(
        Guid categoryId,
        string code,
        string nameEn,
        string nameAr,
        string? value,
        int displayOrder,
        string createdBy)
    {
        var entity = new LookupItem
        {
            CategoryId = categoryId,
            Code = code.ToUpperInvariant(),
            NameEn = nameEn,
            NameAr = nameAr,
            Value = value,
            DisplayOrder = displayOrder
        };

        entity.SetAudit(createdBy);
        entity.AddDomainEvent(new LookupItemCreatedEvent(entity.Id, categoryId, entity.Code));
        return entity;
    }

    public void Update(
        string nameEn,
        string nameAr,
        string? value,
        int displayOrder,
        bool isActive,
        string updatedBy)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        Value = value;
        DisplayOrder = displayOrder;
        IsActive = isActive;
        SetAudit(updatedBy);
        AddDomainEvent(new LookupItemUpdatedEvent(Id, CategoryId, Code));
    }

    public void Delete(string deletedBy)
    {
        SoftDelete();
        SetAudit(deletedBy);
        AddDomainEvent(new LookupItemDeletedEvent(Id, CategoryId, Code));
    }
}

