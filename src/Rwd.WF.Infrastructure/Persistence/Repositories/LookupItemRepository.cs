using Microsoft.EntityFrameworkCore;
using Rwd.WF.Domain.Entities;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Infrastructure.Persistence.Repositories;

public class LookupItemRepository(
    AppDbContext writeContext,
    WorkflowReadDbContext readContext) : ILookupItemRepository
{
    public async Task<LookupItem?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await readContext.LookupItems.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LookupItem>> GetAllAsync(CancellationToken ct = default)
        => await readContext.LookupItems
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public async Task<bool> ExistsByCodeAsync(Guid categoryId, string code, Guid? excludeId = null, CancellationToken ct = default)
        => await readContext.LookupItems.AnyAsync(x =>
            x.CategoryId == categoryId &&
            x.Code == code.ToUpperInvariant() &&
            (excludeId == null || x.Id != excludeId), ct);

    public async Task<LookupItem?> GetWithCategoryAsync(Guid id, CancellationToken ct = default)
        => await readContext.LookupItems
            .Include(x => x.Category)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LookupItem>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default)
        => await readContext.LookupItems
            .Where(x => x.CategoryId == categoryId)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public async Task AddAsync(LookupItem entity, CancellationToken ct = default)
        => await writeContext.LookupItems.AddAsync(entity, ct);

    public void Update(LookupItem entity) => writeContext.LookupItems.Update(entity);
    public void Remove(LookupItem entity) => writeContext.LookupItems.Remove(entity);
}

