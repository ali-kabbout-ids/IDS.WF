using Microsoft.EntityFrameworkCore;
using Rwd.WF.Domain.Entities;
using Rwd.WF.Domain.Repositories;

namespace Rwd.WF.Infrastructure.Persistence.Repositories;

public class LookupCategoryRepository(
    AppDbContext writeContext,
    WorkflowReadDbContext readContext) : ILookupCategoryRepository
{
    public async Task<LookupCategory?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await readContext.LookupCategories.FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LookupCategory>> GetAllAsync(CancellationToken ct = default)
        => await readContext.LookupCategories.OrderBy(x => x.DisplayOrder).ToListAsync(ct);

    public async Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default)
        => await readContext.LookupCategories.AnyAsync(x =>
            x.Code == code.ToUpperInvariant() && (excludeId == null || x.Id != excludeId), ct);

    public async Task<LookupCategory?> GetWithItemsAsync(Guid id, CancellationToken ct = default)
        => await readContext.LookupCategories
            .Include(x => x.Items.Where(i => !i.IsDeleted))
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public async Task<IReadOnlyList<LookupCategory>> GetActiveAsync(CancellationToken ct = default)
        => await readContext.LookupCategories
            .Where(x => x.IsActive)
            .OrderBy(x => x.DisplayOrder)
            .ToListAsync(ct);

    public async Task AddAsync(LookupCategory entity, CancellationToken ct = default)
        => await writeContext.LookupCategories.AddAsync(entity, ct);

    public void Update(LookupCategory entity) => writeContext.LookupCategories.Update(entity);
    public void Remove(LookupCategory entity) => writeContext.LookupCategories.Remove(entity);
}

