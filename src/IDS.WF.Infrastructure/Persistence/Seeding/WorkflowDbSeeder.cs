using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Rwd.WF.Infrastructure.Persistence.Seeding;

public sealed class WorkflowDbSeeder(
    WorkflowWriteDbContext writeDbContext,
    ILookupDefaultsSeeder lookupDefaultsSeeder,
    ILogger<WorkflowDbSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Applying workflow migrations and seeding defaults.");

        await writeDbContext.Database.MigrateAsync(cancellationToken);
        await lookupDefaultsSeeder.SeedAsync(writeDbContext, cancellationToken);

        logger.LogInformation("Workflow database seeding completed.");
    }
}
