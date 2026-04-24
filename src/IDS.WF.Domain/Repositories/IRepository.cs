using Rwd.WF.Domain.Common;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Domain.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
    void Update(T entity);
    void Remove(T entity);
}

public interface ILookupCategoryRepository : IRepository<LookupCategory>
{
    Task<bool> ExistsByCodeAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<LookupCategory?> GetWithItemsAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LookupCategory>> GetActiveAsync(CancellationToken ct = default);
}

public interface ILookupItemRepository : IRepository<LookupItem>
{
    Task<bool> ExistsByCodeAsync(Guid categoryId, string code, Guid? excludeId = null, CancellationToken ct = default);
    Task<LookupItem?> GetWithCategoryAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<LookupItem>> GetByCategoryAsync(Guid categoryId, CancellationToken ct = default);
}

