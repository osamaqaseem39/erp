using ERP.Api.GraphQL;
using ERP.Api.Middlewares;
using ERP.Api.Services;
using ERP.Api.Mappers;
using ERP.Api.Validators;
using ERP.Application.Core;
using ERP.Infrastructure.Database;
using ERP.Infrastructure.Interfaces;
using ERP.Infrastructure.Repositories.Core;
using ERP.Infrastructure.Services;
using ERP.Shared.Constants;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/erp-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<ERPDbContext>(options =>
    options.UseSqlServer(AppConstants.SQL_CONNECTION_STRING));

// Add Dapper and Stored Procedure Executor
builder.Services.AddScoped<IStoredProcedureExecutor>(provider =>
    new StoredProcedureExecutor(AppConstants.SQL_CONNECTION_STRING));

// Add Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
    ConnectionMultiplexer.Connect(AppConstants.REDIS_CONNECTION_STRING));

// Add Repositories
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Add Services
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAuditService, AuditService>();

// Add API Services
builder.Services.AddScoped<ITenantApiService, TenantApiService>();
builder.Services.AddScoped<IAuthApiService, AuthApiService>();

// Add Mappers
builder.Services.AddScoped<TenantMapper>();

// Add Validators
builder.Services.AddScoped<CreateTenantValidator>();
builder.Services.AddScoped<UpdateTenantValidator>();
builder.Services.AddScoped<LoginValidator>();
builder.Services.AddScoped<RefreshTokenValidator>();

// Add FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateTenantValidator>();

// Add HttpContextAccessor for audit logging
builder.Services.AddHttpContextAccessor();

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = AppConstants.JWT_ISSUER,
            ValidAudience = AppConstants.JWT_AUDIENCE,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(AppConstants.JWT_SECRET_KEY)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// Add GraphQL
builder.Services
    .AddGraphQLServer()
    .AddQueryType<CoreQuery>()
    .AddMutationType<CoreMutation>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "ERP SaaS API", Version = "v1" });
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

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<TenantMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGraphQL();

// Ensure database is created
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ERPDbContext>();
    context.Database.EnsureCreated();
}

app.Run(); 