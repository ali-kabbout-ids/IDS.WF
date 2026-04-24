namespace Rwd.WF.Infrastructure.Persistence.Seeding;

public interface ILookupDefaultsSeeder
{
    Task SeedAsync(WorkflowWriteDbContext context, CancellationToken cancellationToken = default);
}
