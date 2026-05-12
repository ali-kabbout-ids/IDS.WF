using System.Reflection;
using Asp.Versioning;
using Elsa.Extensions;
using Elsa.Persistence.EFCore.Extensions;
using Elsa.Persistence.EFCore.Modules.Management;
using Elsa.Persistence.EFCore.Modules.Runtime;
using FastEndpoints.Swagger;
using FluentValidation;
using MassTransit;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
// using StackExchange.Redis;
using Rwd.WF.API.Authorization;
using Rwd.WF.API.Middleware;
using Rwd.WF.Application.Common.Behaviors;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.Features.LookupCategories.Commands.Create;
using Rwd.WF.Domain.Repositories;
using Rwd.WF.Infrastructure;
using Rwd.WF.Infrastructure.Cache;
using Rwd.WF.Infrastructure.Identity;
using Rwd.WF.Infrastructure.Messaging.Consumers;
using Rwd.WF.Infrastructure.Persistence;
using Rwd.WF.Infrastructure.ElsaActivities;
using Rwd.WF.Infrastructure.Workflows;
using Rwd.WF.Infrastructure.Persistence.Repositories;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Elsa /elsa/api/* auth: must run before AddElsa so FastEndpoints register without security (see Elsa docs).
var requireElsaApiAuth = builder.Configuration.GetValue<bool?>("Elsa:Api:RequireAuthentication")
    ?? !builder.Environment.IsDevelopment();

if (!requireElsaApiAuth)
    TryDisableElsaEndpointSecurity();

// ?? Domain / Application ??????????????????????????????????????????????????????
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(Rwd.WF.Application.Features.LookupCategories.Commands.Create.CreateLookupCategoryCommand).Assembly);
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
    cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(
    typeof(Rwd.WF.Application.Features.LookupCategories.Commands.Create.CreateLookupCategoryCommandValidator).Assembly);

// ?? Infrastructure ????????????????????????????????????????????????????????????
builder.Services.AddInfrastructureServices(builder.Configuration);

builder.Services.AddElsa(elsa =>
{
    var elsaConn = builder.Configuration.GetConnectionString("Elsa")!;

    elsa.UseWorkflowManagement(management =>
    {
        management.UseEntityFrameworkCore(ef =>
        {
            ef.UsePostgreSql(elsaConn);
            ef.RunMigrations = true;
        });
    });

    elsa.UseWorkflowRuntime(runtime =>
    {
        runtime.UseEntityFrameworkCore(ef =>
        {
            ef.UsePostgreSql(elsaConn);
            ef.RunMigrations = true;
        });
    });

    elsa.UseScheduling();
    elsa.UseHttp();
    elsa.UseWorkflowsApi();
    elsa.AddSwagger();
    elsa.AddActivity<GenericUserTaskActivity>();
    elsa.AddWorkflow<SaleTransactionPocWorkflow>();
});

// builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
//     ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
// builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ICacheService, NoOpCacheService>();

var mtLicense = Environment.GetEnvironmentVariable("MT_LICENSE");
var mtLicensePath = Environment.GetEnvironmentVariable("MT_LICENSE_PATH");
var canStartMassTransit = !builder.Environment.IsDevelopment() ||
                          !string.IsNullOrWhiteSpace(mtLicense) ||
                          !string.IsNullOrWhiteSpace(mtLicensePath);

if (canStartMassTransit)
{
    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumersFromNamespaceContaining<Placeholder>();
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(
                builder.Configuration["RabbitMQ:Host"],
                builder.Configuration["RabbitMQ:VHost"],
                h =>
                {
                    h.Username(builder.Configuration["RabbitMQ:Username"]!);
                    h.Password(builder.Configuration["RabbitMQ:Password"]!);
                });

            cfg.ConfigureEndpoints(ctx);
        });
    });
}

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ?? Auth ??????????????????????????????????????????????????????????????????????
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.Authority = builder.Configuration["IAM:Authority"];
        opts.Audience = builder.Configuration["IAM:Audience"];
        opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
        opts.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                // Studio sends "Authorization: ApiKey" with no key; do not run JWT for anonymous Elsa API.
                if (!requireElsaApiAuth && context.Request.Path.StartsWithSegments("/elsa"))
                    context.Token = null;
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy(PolicyNames.LookupRead,
        p => p.AddRequirements(new PermissionRequirement(Permissions.LookupRead)))
    .AddPolicy(PolicyNames.LookupCreate,
        p => p.AddRequirements(new PermissionRequirement(Permissions.LookupCreate)))
    .AddPolicy(PolicyNames.LookupUpdate,
        p => p.AddRequirements(new PermissionRequirement(Permissions.LookupUpdate)))
    .AddPolicy(PolicyNames.LookupDelete,
        p => p.AddRequirements(new PermissionRequirement(Permissions.LookupDelete)));

// CORS: Elsa Studio / Blazor WASM (e.g. https://localhost:44314) calls API on another port → browser requires Allow-Origin.
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
if (corsOrigins.Length == 0 && builder.Environment.IsDevelopment())
    corsOrigins = ["https://localhost:44314", "http://localhost:44314"];

builder.Services.AddCors(opts =>
{
    opts.AddPolicy("ElsaStudio", policy =>
    {
        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// ?? API ???????????????????????????????????????????????????????????????????????
builder.Services.AddControllers();

builder.Services.AddApiVersioning(opts =>
{
    opts.DefaultApiVersion = new ApiVersion(1, 0);
    opts.AssumeDefaultVersionWhenUnspecified = true;
    opts.ReportApiVersions = true;
    opts.ApiVersionReader = new UrlSegmentApiVersionReader();
});

// ?? Swagger ???????????????????????????????????????????????????????????????????
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(opts =>
{
    // Avoid schema-id collisions (e.g. Elsa endpoints often have same short type names).
    opts.CustomSchemaIds(t => (t.FullName ?? t.Name).Replace('+', '.'));

    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "IDS Workflow Management API",
        Version = "v1",
        Description = "Workflow & Lookup Management Service – IDS Platform",
        Contact = new OpenApiContact
        {
            Name = "IDS Platform Team",
            Email = "platform@ids.com"
        }
    });

    // ── JWT Bearer ────────────────────────────────────────────────────────────
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: eyJhbGci..."
    });

    opts.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id   = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });

    opts.UseInlineDefinitionsForEnums();

    // MVC swagger should only include MVC controller actions.
    // Elsa's endpoints are FastEndpoints and are documented separately at /elsa/swagger.
    opts.DocInclusionPredicate((_, apiDesc) =>
    {
        var path = apiDesc.RelativePath ?? string.Empty;
        if (path.StartsWith("internal/", StringComparison.OrdinalIgnoreCase))
            return false;

        return apiDesc.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) &&
               !string.IsNullOrWhiteSpace(controller);
    });

    opts.TagActionsBy(api =>
    {
        if (!string.IsNullOrWhiteSpace(api.GroupName))
            return [api.GroupName];

        return api.ActionDescriptor.RouteValues.TryGetValue("controller", out var controller) &&
               !string.IsNullOrWhiteSpace(controller)
            ? [controller]
            : ["Endpoints"];
    });
});

// ?? Logging ???????????????????????????????????????????????????????????????????
builder.Host.UseSerilog((ctx, cfg) => cfg
    .ReadFrom.Configuration(ctx.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.Seq(ctx.Configuration["Seq:Url"]!));

var app = builder.Build();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("ElsaStudio");

if (!requireElsaApiAuth)
{
    app.Use(async (context, next) =>
    {
        if (context.Request.Path.StartsWithSegments("/elsa"))
            context.Request.Headers.Remove("Authorization");
        await next();
    });
}

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Rwd.WF API v1");
        opts.RoutePrefix = string.Empty;
        opts.DocumentTitle = "IDS Workflow API";
        opts.DisplayRequestDuration();
        opts.EnableDeepLinking();
        opts.DefaultModelsExpandDepth(-1);
    });

    // Elsa (FastEndpoints/NSwag) Swagger & UI.
    // This is separate from Swashbuckle to avoid mixing MVC and FastEndpoints pipelines.
    app.UseSwaggerGen(
        s =>
        {
            s.Path = "/elsa/swagger/{documentName}/swagger.json";
        },
        ui =>
        {
            ui.Path = "/elsa/swagger";
            ui.DocumentPath = "/elsa/swagger/{documentName}/swagger.json";
        });
}

app.UseWorkflows();        // Elsa HTTP triggers
app.MapWorkflowsApi();     // Elsa REST API (documented via /elsa/swagger in dev/staging)
app.MapControllers();      // Your MVC controllers (appear in Swagger)

// Apply migrations first, then seed
using (var scope = app.Services.CreateScope())
{
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("Startup");

    // 1. Migrate your Application DB
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // 2. Explicitly migrate Elsa's DB
    var managementDb = scope.ServiceProvider.GetRequiredService<ManagementElsaDbContext>();
    await managementDb.Database.MigrateAsync();

    var runtimeDb = scope.ServiceProvider.GetRequiredService<RuntimeElsaDbContext>();
    await runtimeDb.Database.MigrateAsync();

    await LogElsaPostgresTargetsAsync(startupLogger, managementDb, runtimeDb);
}

if (app.Environment.IsDevelopment())
{
    // Open in browser to verify the same DB your tool uses: tables live in schema "Elsa" (capital E).
    app.MapGet("/internal/debug/elsa-persistence", async (ManagementElsaDbContext managementDb) =>
    {
        var conn = managementDb.Database.GetDbConnection();
        await conn.OpenAsync();
        try
        {
            string dbName;
            string user;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT current_database(), current_user;";
                await using var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                dbName = reader.GetString(0);
                user = reader.GetString(1);
            }

            var tables = new List<string>();
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                                    SELECT tablename
                                    FROM pg_tables
                                    WHERE schemaname = 'Elsa'
                                    ORDER BY tablename;
                                    """;
                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    tables.Add(reader.GetString(0));
            }

            return Results.Json(new
            {
                database = dbName,
                user,
                note = "Elsa uses schema \"Elsa\" (quoted, capital E). It is not under public.",
                elsaTableCount = tables.Count,
                tables
            });
        }
        finally
        {
            await conn.CloseAsync();
        }
    }).ExcludeFromDescription();
}

// 3. Clean up the duplicate seed/run calls at the end of your file
await app.Services.SeedInfrastructureAsync();
await app.RunAsync();

static void TryDisableElsaEndpointSecurity()
{
    var asm = Assembly.Load("Elsa.Api.Common");

    // Namespace can move between versions; find by type name.
    var t = asm
        .GetExportedTypes()
        .FirstOrDefault(x => string.Equals(x.Name, "EndpointSecurityOptions", StringComparison.Ordinal))
        ?? asm.GetType("Elsa.Api.Common.Options.EndpointSecurityOptions", throwOnError: false)
        ?? asm.GetType("Elsa.Api.Common.Options.EndpointSecurityOptions, Elsa.Api.Common", throwOnError: false)
        ?? throw new InvalidOperationException("EndpointSecurityOptions type not found in Elsa.Api.Common.");

    var m = t.GetMethod("DisableSecurity", BindingFlags.Public | BindingFlags.Static);
    if (m == null)
        throw new InvalidOperationException("EndpointSecurityOptions.DisableSecurity() not found.");

    m.Invoke(null, null);
}

static async Task LogElsaPostgresTargetsAsync(
    Microsoft.Extensions.Logging.ILogger logger,
    ManagementElsaDbContext managementDb,
    RuntimeElsaDbContext runtimeDb)
{
    async Task LogOneAsync(string label, DbContext ctx)
    {
        var conn = ctx.Database.GetDbConnection();
        var wasOpen = conn.State == System.Data.ConnectionState.Open;
        if (!wasOpen)
            await conn.OpenAsync();
        try
        {
            string db;
            string user;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT current_database(), current_user;";
                await using var reader = await cmd.ExecuteReaderAsync();
                await reader.ReadAsync();
                db = reader.GetString(0);
                user = reader.GetString(1);
            }

            long count;
            await using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = """
                                    SELECT COUNT(*)::bigint
                                    FROM information_schema.tables
                                    WHERE table_schema = 'Elsa';
                                    """;
                var countObj = await cmd.ExecuteScalarAsync();
                count = countObj is long l ? l : Convert.ToInt64(countObj ?? 0L);
            }

            logger.LogInformation(
                "Elsa EF {Label}: database={Database} user={User} tables in schema \"Elsa\"={Count}",
                label, db, user, count);
        }
        finally
        {
            if (!wasOpen)
                await conn.CloseAsync();
        }
    }

    await LogOneAsync("Management", managementDb);
    await LogOneAsync("Runtime", runtimeDb);
}
