namespace Rwd.WF.Infrastructure.Persistence.Seeding;

public interface ILookupDefaultsSeeder
{
    Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default);
}
