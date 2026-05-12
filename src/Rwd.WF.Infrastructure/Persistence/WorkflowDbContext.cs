using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence;

public class WorkflowDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<LookupCategory> LookupCategories => Set<LookupCategory>();
    public DbSet<LookupItem> LookupItems => Set<LookupItem>();
    public DbSet<FormApplication> Applications => Set<FormApplication>();
    public DbSet<WorkflowTask> WorkflowTasks => Set<WorkflowTask>();
    public DbSet<SubWorkflowResult> SubWorkflowResults => Set<SubWorkflowResult>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.HasDefaultSchema("workflow");

        builder.Entity<LookupCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<LookupItem>().HasQueryFilter(e => !e.IsDeleted);
    }
}

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : WorkflowDbContext(options), IAppDbContext
{
}

public sealed class WorkflowReadDbContext(DbContextOptions<WorkflowReadDbContext> options)
    : WorkflowDbContext(options)
{
}

