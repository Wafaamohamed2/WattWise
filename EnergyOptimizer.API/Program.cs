using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Infrastructure.Data;
using Serilog;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.Service.Services;
using EnergyOptimizer.API.Services;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Repositories;
using EnergyOptimizer.Core.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using EnergyOptimizer.API.WebServices;
using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.Service.Services.Abstract;
using EnergyOptimizer.Service.Services.Implementation;
using EnergyOptimizer.API.Helpers;
using EnergyOptimizer.Core.Features.AI.Commands;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using static EnergyOptimizer.API.Services.EnergyReadingSimulatorService;
using EnergyOptimizer.API.Middleware;
using Asp.Versioning;
using EnergyOptimizer.API.Swagger;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Serilog 
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/energy-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers 
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition =
            System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.PropertyNamingPolicy =
           System.Text.Json.JsonNamingPolicy.CamelCase;
    });

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value!.Errors.Count > 0)
            .SelectMany(x => x.Value!.Errors)
            .Select(x => x.ErrorMessage)
            .ToArray();

        return new BadRequestObjectResult(new ApiResponse(400, "Validation Failed", errors));
    };
});

builder.Services.AddEndpointsApiExplorer();

// API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
})
.AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Swagger Registration
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddSwaggerGen();

// Generic Repository 
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Database 
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContextFactory<EnergyDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

// SignalR 
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Identity 
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<EnergyDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication 
var jwtSettings = builder.Configuration.GetSection("Jwt");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                       Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            // 1. Try HttpOnly cookie first (browser pages)
            if (ctx.Request.Cookies.TryGetValue("access_token", out var cookieToken)
                && !string.IsNullOrEmpty(cookieToken))
            {
                ctx.Token = cookieToken;
                return Task.CompletedTask;
            }

            // 2. Fallback to Authorization header (Swagger / API clients)
            var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                ctx.Token = authHeader["Bearer ".Length..];
                return Task.CompletedTask;
            }

            // 3. SignalR sends token as query param
            var accessToken = ctx.Request.Query["access_token"];
            var path = ctx.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/energyhub"))
                ctx.Token = accessToken;

            return Task.CompletedTask;
        }
    };
});

// IJwtTokenService 
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Rate Limiting 
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("auth", limiterOptions =>
    {
        limiterOptions.Window = TimeSpan.FromMinutes(1);
        limiterOptions.PermitLimit = 10;   // 10 requests/min per client
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Strongly-typed options 
builder.Services.Configure<AIAnalysisOptions>(
    builder.Configuration.GetSection(AIAnalysisOptions.SectionName));

builder.Services.Configure<SimulationOptions>(
    builder.Configuration.GetSection(SimulationOptions.SectionName));

// MediatR 
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(typeof(RunGlobalAnalysisCommand).Assembly);
});

// Background Services 
builder.Services.AddHostedService<EnergyReadingSimulatorService>();
builder.Services.AddHostedService<AlertDetectionService>();
builder.Services.AddHostedService<AIAnalysisBackgroundService>();

// Application Services 
builder.Services.AddScoped<IEnergyHubService, EnergyHubService>();
builder.Services.AddTransient<DataSeedingService>();

// Gemini / AI
builder.Services.Configure<GeminiSettings>(
    builder.Configuration.GetSection("Gemini"));

builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddScoped<IPatternDetectionService, PatternDetectionService>();
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IDataCleanupService, DataCleanupService>();

//  AutoMapper 
builder.Services.AddAutoMapper(typeof(MappingProfiles));

//  CORS
var allowedOrigins = builder.Configuration
    .GetSection("AllowedOrigins")
    .Get<string[]>()
    ?? new[] { "https://localhost:7083", "http://localhost:5167" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            var url = $"/swagger/{description.GroupName}/swagger.json";
            var name = description.GroupName.ToUpperInvariant();
            options.SwaggerEndpoint(url, name);
        }
    });
}

app.UseSerilogRequestLogging();

// Handle non-versioned API requests by rewriting to /api/v1/...
app.Use(async (context, next) =>
{
    var path = context.Request.Path.Value;
    if (path != null && 
        path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) && 
        !path.StartsWith("/api/v", StringComparison.OrdinalIgnoreCase))
    {
        var remainingPath = path["/api/".Length..];
        context.Request.Path = $"/api/v1/{remainingPath}";
    }
    await next();
});

app.UseCors("AllowFrontend");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<EnergyHub>("/energyhub");

app.Run();