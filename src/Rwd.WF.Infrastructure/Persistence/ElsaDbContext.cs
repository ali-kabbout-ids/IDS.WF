//using Elsa.Persistence.EFCore;
//using Microsoft.EntityFrameworkCore;

//namespace Rwd.WF.Infrastructure.Persistence;

///// <summary>
///// Custom Elsa DbContext that inherits from Elsa's base context.
///// This allows you to manage Elsa's schema within your Infrastructure project
///// and separate its migrations from your Application context.
///// </summary>
//public class ElsaDbContext : ElsaDbContextBase
//{
//    public ElsaDbContext(DbContextOptions<ElsaDbContext> options) : base(options)
//    {
//    }

//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//    {
//        // Add any specific Npgsql tuning if required, 
//        // though usually handled in Program.cs
//        base.OnConfiguring(optionsBuilder);
//    }

//    protected override void OnModelCreating(ModelBuilder modelBuilder)
//    {
//        base.OnModelCreating(modelBuilder);

//        // Optional: If you want Elsa tables to live in a specific schema 
//        // to keep your DB clean (e.g., "elsa"), uncomment the line below:
//        // modelBuilder.HasDefaultSchema("elsa");
//    }
//}

using Elsa.Persistence.EFCore;
using Microsoft.EntityFrameworkCore;

namespace Rwd.WF.Infrastructure.Persistence;

public class ElsaDbContext : ElsaDbContextBase
{
    public ElsaDbContext(
        DbContextOptions<ElsaDbContext> options,
        IServiceProvider serviceProvider) // ✅ Add this
        : base(options, serviceProvider)   // ✅ Pass it here
    {
    }
}