using Microsoft.EntityFrameworkCore;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<FormApplication> Applications { get; }
    DbSet<WorkflowTask> WorkflowTasks { get; }
    DbSet<SubWorkflowResult> SubWorkflowResults { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
