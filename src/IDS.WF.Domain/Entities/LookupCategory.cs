using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Events;

namespace Rwd.WF.Domain.Entities;

public sealed class LookupCategory : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public string NameEn { get; private set; } = string.Empty;
    public string NameAr { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;
    public int DisplayOrder { get; private set; }

    private readonly List<LookupItem> _items = [];
    public IReadOnlyCollection<LookupItem> Items => _items.AsReadOnly();

    private LookupCategory() { } // EF

    public static LookupCategory Create(
        string code,
        string nameEn,
        string nameAr,
        string? description,
        int displayOrder,
        string createdBy)
    {
        var entity = new LookupCategory
        {
            Code = code.ToUpperInvariant(),
            NameEn = nameEn,
            NameAr = nameAr,
            Description = description,
            DisplayOrder = displayOrder
        };

        entity.SetAudit(createdBy);
        entity.AddDomainEvent(new LookupCategoryCreatedEvent(entity.Id, entity.Code));
        return entity;
    }

    public void Update(
        string nameEn,
        string nameAr,
        string? description,
        int displayOrder,
        bool isActive,
        string updatedBy)
    {
        NameEn = nameEn;
        NameAr = nameAr;
        Description = description;
        DisplayOrder = displayOrder;
        IsActive = isActive;

        SetAudit(updatedBy);
        AddDomainEvent(new LookupCategoryUpdatedEvent(Id, Code));
    }

    public void Delete(string deletedBy)
    {
        SoftDelete();
        SetAudit(deletedBy);
        AddDomainEvent(new LookupCategoryDeletedEvent(Id, Code));
    }
}

