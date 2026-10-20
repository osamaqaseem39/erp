using Microsoft.AspNetCore.DataProtection;
using System.Reflection;
using System.Threading.RateLimiting;
using ERP.Api.Extensions;
using ERP.Api.GraphQL;
using ERP.Api.GraphQL.Admin;
using ERP.Api.GraphQL.Core;
using ERP.Api.GraphQL.CRM;
using ERP.Api.GraphQL.Finance;
using ERP.Api.GraphQL.HRM;
using ERP.Api.GraphQL.Inventory;
using ERP.Api.GraphQL.Manufacturing;
using ERP.Api.GraphQL.Procurement;
using ERP.Api.GraphQL.Projects;
using ERP.Api.GraphQL.Reports;
using ERP.Api.GraphQL.Sales;
using ERP.Api.Middlewares;
using ERP.Api.Validators;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Services;
using ERP.Shared.Configuration;
using ERP.Shared.Helpers;
using FluentValidation;
using HotChocolate.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("Jwt configuration section is required.");
JwtHelper.Configure(jwtSettings);

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddControllers(options =>
    {
        // Every endpoint requires the permission derived from its route (see EndpointPermissions).
        options.Filters.Add<ERP.Api.Security.Permissions.PermissionAuthorizationFilter>();
    })
    .ConfigureApiBehaviorOptions(options =>
    {
        // Model-binding failures (malformed JSON, wrong types) use the same body as every other error.
        options.InvalidModelStateResponseFactory = context =>
        {
            var errors = context.ModelState
                .Where(entry => entry.Value?.Errors.Count > 0)
                .ToDictionary(
                    entry => ERP.Api.Errors.ApiErrorMapper.ToCamelCasePath(entry.Key.TrimStart('$', '.')),
                    entry => entry.Value!.Errors
                        .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "The value is not valid." : e.ErrorMessage)
                        .ToArray());
            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(new ERP.Shared.DTOs.ApiError
            {
                Status = 400,
                ErrorCode = ERP.Shared.DTOs.ApiErrorCodes.ValidationError,
                Message = "The request is not valid.",
                Errors = errors,
                TraceId = context.HttpContext.TraceIdentifier,
            });
        };
    });
builder.Services.AddHttpContextAccessor();
// Keys that encrypt sensitive settings. Back up this folder: without it, stored secrets must be re-entered.
var dataProtectionKeys = builder.Configuration["DataProtection:KeysPath"] ?? System.IO.Path.Combine(builder.Environment.ContentRootPath, "App_Data", "keys");
builder.Services.AddDataProtection()
    .SetApplicationName("ERP")
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeys));
builder.Services.AddMemoryCache();

// Every change to tenant data is audited in the same transaction (see AuditSaveChangesInterceptor).
builder.Services.AddScoped<ERP.Shared.Abstractions.IAuditContext, ERP.Api.Security.HttpAuditContext>();
builder.Services.AddScoped<ERP.Infrastructure.Auditing.AuditSaveChangesInterceptor>();
builder.Services.AddScoped<ERP.Infrastructure.Auditing.TenantGuardInterceptor>();
builder.Services.AddSingleton<ERP.Infrastructure.Logging.ISystemLogWriter, ERP.Infrastructure.Logging.SystemLogWriter>();
// Permissions: catalog from the endpoints, per-user cache cleared when roles change.
builder.Services.AddSingleton<ERP.Api.Security.Permissions.EndpointPermissions>();
builder.Services.AddSingleton<ERP.Infrastructure.Security.IPermissionCatalog>(sp =>
    new ERP.Api.Security.Permissions.CompositePermissionCatalog(
        sp.GetRequiredService<ERP.Api.Security.Permissions.EndpointPermissions>(),
        ERP.Api.Security.Permissions.ReportPermissionCatalog.All));
builder.Services.AddSingleton<ERP.Infrastructure.Security.IEffectivePermissions, ERP.Infrastructure.Security.EffectivePermissions>();
builder.Services.AddSingleton<ERP.Infrastructure.Security.PermissionCacheInvalidator>();
builder.Services.AddScoped<ERP.Infrastructure.Security.IRbacSeeder, ERP.Infrastructure.Security.RbacSeeder>();
builder.Services.AddDbContext<ERPDbContext>((sp, options) =>
    options.UseSqlServer(connectionString)
        // Guard first so the audit trail records the tenant the row actually lands in.
        .AddInterceptors(
            sp.GetRequiredService<ERP.Infrastructure.Auditing.TenantGuardInterceptor>(),
            sp.GetRequiredService<ERP.Infrastructure.Auditing.AuditSaveChangesInterceptor>(),
            sp.GetRequiredService<ERP.Infrastructure.Security.PermissionCacheInvalidator>()));

builder.Services.AddErpInfrastructure(connectionString);
builder.Services.AddErpCoreModule();
builder.Services.AddErpAdminModule();
builder.Services.AddErpSalesModule();
builder.Services.AddErpCrmModule();
builder.Services.AddErpFinanceModule();
builder.Services.AddErpInventoryModule();
builder.Services.AddErpHrmModule();
builder.Services.AddErpProcurementModule();
builder.Services.AddErpManufacturingModule();
builder.Services.AddErpProjectsModule();
builder.Services.AddErpReportsModule();

builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();

var redisEnabled = builder.Configuration.GetValue("Redis:Enabled", false);
var redisConnection = builder.Configuration.GetConnectionString("Redis");
if (redisEnabled && !string.IsNullOrWhiteSpace(redisConnection))
{
    builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
        ConnectionMultiplexer.Connect(redisConnection));
}
else
{
    Log.Information("Redis disabled or not configured — skipping Redis connection.");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.Zero,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("auth", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (corsOrigins.Length == 0 || builder.Environment.IsDevelopment())
        {
            var origins = corsOrigins.Length > 0
                ? corsOrigins
                : new[] { "http://localhost:3000", "http://localhost:5173", "https://localhost:3000", "https://localhost:5173" };
            policy.WithOrigins(origins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else
        {
            policy.WithOrigins(corsOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
    });
});

builder.Services
    .AddGraphQLServer()
    .AddAuthorization()
    .AddQueryType<CoreQuery>()
    .AddMutationType<CoreMutation>()
    .AddTypeExtension<SalesQueries>()
    .AddTypeExtension<SalesMutations>()
    .AddTypeExtension<HRMQueries>()
    .AddTypeExtension<HRMMutations>()
    .AddTypeExtension<EmployeeQuery>()
    .AddTypeExtension<EmployeeMutation>()
    .AddTypeExtension<InventoryQueries>()
    .AddTypeExtension<InventoryMutations>()
    .AddTypeExtension<ProjectsQueries>()
    .AddTypeExtension<ProjectsMutations>()
    .AddTypeExtension<ManufacturingQueries>()
    .AddTypeExtension<ManufacturingMutations>()
    .AddTypeExtension<ReportsQueries>()
    .AddTypeExtension<ReportsMutations>()
    .AddTypeExtension<AdminQueries>()
    .AddTypeExtension<AdminMutations>()
    .AddTypeExtension<CRMQueries>()
    .AddTypeExtension<CRMMutations>()
    .AddTypeExtension<FinanceQueries>()
    .AddTypeExtension<FinanceMutations>()
    .AddTypeExtension<ProcurementQueries>()
    .AddTypeExtension<ProcurementMutations>()
    .AddTypeExtension<CoreModuleQueries>()
    .AddTypeExtension<CoreModuleMutations>()
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = builder.Environment.IsDevelopment());

builder.Services.AddEndpointsApiExplorer();
// One Swagger document per module, keyed by the first route segment after /api/.
// "v1" keeps the full API in one document; auth, setup and tenants are grouped as "platform".
var swaggerModules = new (string Key, string Title, string[] Prefixes)[]
{
    ("platform", "Platform (Auth, Setup, Tenants)", ["auth", "setup", "tenants"]),
    ("admin", "Admin", ["admin"]),
    ("core", "Core", ["core"]),
    ("finance", "Finance", ["finance"]),
    ("crm", "CRM", ["crm"]),
    ("sales", "Sales", ["sales"]),
    ("inventory", "Inventory", ["inventory"]),
    ("procurement", "Procurement", ["procurement"]),
    ("manufacturing", "Manufacturing", ["manufacturing"]),
    ("hrm", "HRM", ["hrm"]),
    ("projects", "Projects", ["projects"]),
    ("reports", "Reports", ["reports"]),
};

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ERP SaaS API", Version = "v1" });
    foreach (var m in swaggerModules)
        c.SwaggerDoc(m.Key, new() { Title = $"ERP SaaS API - {m.Title}", Version = "v1" });

    // Admin.UserDto vs Core.UserDto (and similar) collide on short type names.
    c.CustomSchemaIds(t => t.FullName?.Replace('+', '.') ?? t.Name);

    c.DocInclusionPredicate((docName, apiDesc) =>
    {
        if (docName == "v1") return true;
        var path = apiDesc.RelativePath ?? string.Empty;
        var segment = path.StartsWith("api/", StringComparison.OrdinalIgnoreCase)
            ? path[4..].Split('/', '?')[0]
            : string.Empty;
        var module = swaggerModules.FirstOrDefault(m => m.Key == docName);
        return module.Prefixes?.Contains(segment, StringComparer.OrdinalIgnoreCase) == true;
    });

    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddScoped<DatabaseDeploymentService>(sp =>
{
    var context = sp.GetRequiredService<ERPDbContext>();
    var logger = sp.GetRequiredService<ILogger<DatabaseDeploymentService>>();
    return new DatabaseDeploymentService(context, connectionString, logger);
});

var app = builder.Build();

if (app.Environment.IsDevelopment() || builder.Configuration.GetValue("Swagger:Enabled", false))
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        foreach (var m in swaggerModules)
            c.SwaggerEndpoint($"/swagger/{m.Key}/swagger.json", m.Title);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "All modules");
        c.DocumentTitle = "ERP SaaS API";
    });
    // Runs before authentication so the bare root lands on the docs instead of a 401.
    app.Use(async (ctx, next) =>
    {
        if (ctx.Request.Path == "/") { ctx.Response.Redirect("/swagger"); return; }
        await next();
    });
}

app.UseHttpsRedirection();
app.UseCors("DefaultCors");
app.UseRateLimiter();
app.UseMiddleware<ExceptionMiddleware>();
app.UseAuthentication();
app.UseMiddleware<TenantMiddleware>();
app.UseAuthorization();

app.MapControllers();
// GraphQL bypasses controller permissions, so it is limited to platform administrators.
app.MapGraphQL().RequireAuthorization(policy => policy.RequireRole(ERP.Api.Security.CurrentUser.SuperAdminRoles));

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var failOnMigration = builder.Configuration.GetValue("Database:FailOnMigrationError", !app.Environment.IsDevelopment());
    var deploySql = builder.Configuration.GetValue("Database:DeploySqlScripts", true);
    var failOnSqlDeploy = builder.Configuration.GetValue("Database:FailOnSqlDeployError", false);

    try
    {
        context.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Database migration failed");
        if (failOnMigration)
            throw;
        logger.LogWarning("Continuing without EnsureCreated fallback to avoid schema drift.");
    }

    if (deploySql)
    {
        try
        {
            var deploymentService = scope.ServiceProvider.GetRequiredService<DatabaseDeploymentService>();
            deploymentService.DeployAllAsync(deployTables: true, deployStoredProcedures: true)
                .GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deploying SQL scripts");
            if (failOnSqlDeploy)
                throw;
        }
    }

    // Keep each organization's permissions and built-in roles in step with the endpoints.
    try
    {
        scope.ServiceProvider.GetRequiredService<ERP.Infrastructure.Security.IRbacSeeder>()
            .SeedAllTenantsAsync().GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Permission seeding failed");
        if (failOnMigration)
            throw;
    }
}

app.Run();

public partial class Program { }
