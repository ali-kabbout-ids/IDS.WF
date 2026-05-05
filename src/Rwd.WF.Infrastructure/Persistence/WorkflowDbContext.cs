using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Rwd.WF.Domain.Entities;

namespace Rwd.WF.Infrastructure.Persistence;

public class WorkflowDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<LookupCategory> LookupCategories => Set<LookupCategory>();
    public DbSet<LookupItem> LookupItems => Set<LookupItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        builder.HasDefaultSchema("workflow");

        builder.Entity<LookupCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<LookupItem>().HasQueryFilter(e => !e.IsDeleted);
    }
}

public sealed class WorkflowWriteDbContext(DbContextOptions<WorkflowWriteDbContext> options)
    : WorkflowDbContext(options)
{
}

public sealed class WorkflowReadDbContext(DbContextOptions<WorkflowReadDbContext> options)
    : WorkflowDbContext(options)
{
}

