using Rwd.WF.Application.Common.Interfaces;

namespace Rwd.WF.Infrastructure.Persistence;

public sealed class UnitOfWork(WorkflowWriteDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken ct = default)
        => context.SaveChangesAsync(ct);
}
