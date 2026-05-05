using Asp.Versioning;
using FluentValidation;
using MassTransit;
using MediatR;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
// using StackExchange.Redis;
using Rwd.WF.API.Authorization;
using Rwd.WF.API.Middleware;
using Rwd.WF.Application.Common.Behaviors;
using Rwd.WF.Application.Common.Interfaces;
using Rwd.WF.Application.Features.LookupCategories.Commands.Create;
using Rwd.WF.Domain.Repositories;
using Rwd.WF.Infrastructure.Cache;
using Rwd.WF.Infrastructure.Identity;
using Rwd.WF.Infrastructure.Messaging.Consumers;
using Rwd.WF.Infrastructure.Persistence;
using Rwd.WF.Infrastructure.Persistence.Repositories;
using Rwd.WF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

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

// builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
//     ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")!));
// builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<ICacheService, NoOpCacheService>();

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

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ?? Auth ??????????????????????????????????????????????????????????????????????
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.Authority = builder.Configuration["IAM:Authority"];
        opts.Audience = builder.Configuration["IAM:Audience"];
        opts.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
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
    opts.SwaggerDoc("v1", new OpenApiInfo
    {
        Title       = "IDS Workflow Management API",
        Version     = "v1",
        Description = "Workflow & Lookup Management Service � IDS Platform",
        Contact     = new OpenApiContact
        {
            Name  = "IDS Platform Team",
            Email = "platform@ids.com"
        }
    });

    // Support JWT Bearer in Swagger UI
    opts.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name         = "Authorization",
        Type         = SecuritySchemeType.Http,
        Scheme       = "bearer",
        BearerFormat = "JWT",
        In           = ParameterLocation.Header,
        Description  = "Enter your JWT token. Example: eyJhbGci..."
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

    // Show enum names instead of numbers
    opts.UseInlineDefinitionsForEnums();

    // Group endpoints by controller
    opts.TagActionsBy(api => [api.GroupName ?? api.ActionDescriptor.RouteValues["controller"]!]);
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
app.UseAuthentication();
app.UseAuthorization();

// ?? Swagger UI (always on in Development, can be toggled for Staging) ?????????
if (app.Environment.IsDevelopment() || app.Environment.IsStaging())
{
    app.UseSwagger();
    app.UseSwaggerUI(opts =>
    {
        opts.SwaggerEndpoint("/swagger/v1/swagger.json", "Rwd.WF API v1");
        opts.RoutePrefix        = string.Empty;
        opts.DocumentTitle      = "IDS Workflow API";
        opts.DisplayRequestDuration();
        opts.EnableDeepLinking();
        opts.DefaultModelsExpandDepth(-1);
    });
}
app.MapControllers();

// Apply migrations first, then seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<WorkflowWriteDbContext>();
    await db.Database.MigrateAsync(); // ← creates tables
}

await app.Services.SeedInfrastructureAsync(); // ← then seed
await app.RunAsync();
