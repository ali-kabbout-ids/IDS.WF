using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Domain.Repositories;
using Rwd.WF.Infrastructure.Persistence;
using Rwd.WF.Infrastructure.Persistence.Repositories;
using Rwd.WF.Infrastructure.Persistence.Seeding;
using Rwd.WF.Infrastructure.Workflow;

namespace Rwd.WF.Infrastructure;

public static class ConfigureServices
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Postgres");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(
                connectionString,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "workflow")));

        services.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        services.AddDbContext<WorkflowReadDbContext>(opts =>
            opts.UseNpgsql(
                connectionString,
                x => x.MigrationsHistoryTable("__EFMigrationsHistory", "workflow"))
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

        services.AddScoped<ILookupCategoryRepository, LookupCategoryRepository>();
        services.AddScoped<ILookupItemRepository, LookupItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ILookupDefaultsSeeder, LookupDefaultsSeeder>();
        services.AddScoped<WorkflowDbSeeder>();

        services.AddScoped<IWorkflowEngine, WorkflowEngine>();

        return services;
    }

    public static async Task SeedInfrastructureAsync(this IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var seeder = scope.ServiceProvider.GetRequiredService<WorkflowDbSeeder>();
        await seeder.SeedAsync(cancellationToken);
    }
}
