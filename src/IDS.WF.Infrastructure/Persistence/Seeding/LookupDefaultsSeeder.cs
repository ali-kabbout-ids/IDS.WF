using Microsoft.EntityFrameworkCore;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence.Seeding;

public sealed class LookupDefaultsSeeder : ILookupDefaultsSeeder
{
    private const string SeedUser = "system-seed";

    public async Task SeedAsync(WorkflowWriteDbContext context, CancellationToken cancellationToken = default)
    {
        var categoryDefinitions = new[]
        {
            new { Code = "WORKFLOW_STATUS", NameEn = "Workflow Status", NameAr = "حالة سير العمل", Description = "Default workflow statuses", DisplayOrder = 1 },
            new { Code = "PRIORITY", NameEn = "Priority", NameAr = "الأولوية", Description = "Default workflow priorities", DisplayOrder = 2 }
        };

        foreach (var definition in categoryDefinitions)
        {
            var exists = await context.LookupCategories
                .IgnoreQueryFilters()
                .AnyAsync(x => x.Code == definition.Code, cancellationToken);

            if (exists)
            {
                continue;
            }

            var category = LookupCategory.Create(
                definition.Code,
                definition.NameEn,
                definition.NameAr,
                definition.Description,
                definition.DisplayOrder,
                SeedUser);

            category.ClearDomainEvents();
            await context.LookupCategories.AddAsync(category, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);

        var categoriesByCode = await context.LookupCategories
            .Where(x => x.Code == "WORKFLOW_STATUS" || x.Code == "PRIORITY")
            .ToDictionaryAsync(x => x.Code, x => x.Id, cancellationToken);

        if (categoriesByCode.TryGetValue("WORKFLOW_STATUS", out var workflowStatusId))
        {
            await SeedItemsAsync(
                context,
                workflowStatusId,
                [
                    ("DRAFT", "Draft", "مسودة", "DRAFT", 1),
                    ("PUBLISHED", "Published", "منشور", "PUBLISHED", 2),
                    ("ARCHIVED", "Archived", "مؤرشف", "ARCHIVED", 3)
                ],
                cancellationToken);
        }

        if (categoriesByCode.TryGetValue("PRIORITY", out var priorityId))
        {
            await SeedItemsAsync(
                context,
                priorityId,
                [
                    ("LOW", "Low", "منخفض", "LOW", 1),
                    ("MEDIUM", "Medium", "متوسط", "MEDIUM", 2),
                    ("HIGH", "High", "مرتفع", "HIGH", 3)
                ],
                cancellationToken);
        }
    }

    private static async Task SeedItemsAsync(
        WorkflowWriteDbContext context,
        Guid categoryId,
        IReadOnlyCollection<(string Code, string NameEn, string NameAr, string? Value, int DisplayOrder)> items,
        CancellationToken cancellationToken)
    {
        foreach (var item in items)
        {
            var exists = await context.LookupItems
                .IgnoreQueryFilters()
                .AnyAsync(x => x.CategoryId == categoryId && x.Code == item.Code, cancellationToken);

            if (exists)
            {
                continue;
            }

            var entity = LookupItem.Create(
                categoryId,
                item.Code,
                item.NameEn,
                item.NameAr,
                item.Value,
                item.DisplayOrder,
                SeedUser);

            entity.ClearDomainEvents();
            await context.LookupItems.AddAsync(entity, cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }
}
