using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Infrastructure.Data;
using Serilog;
using EnergyOptimizer.API.Hubs;
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
using EnergyOptimizer.API.Middleware;
using EnergyOptimizer.API.Helpers;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.RateLimiting;               
using System.Threading.RateLimiting;
using static EnergyOptimizer.API.Services.EnergyReadingSimulatorService;
using ApiResponse = EnergyOptimizer.API.Middleware.ExceptionMiddleware.ApiResponse;

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
builder.Services.AddSwaggerGen();

// Generic Repository 
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Database 
builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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
var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException("JWT Key is not configured in appsettings.json under 'Jwt:Key'.");

if (jwtKey.Length < 32)
    throw new InvalidOperationException("JWT Key must be at least 32 characters for HMAC-SHA256.");


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

    // SignalR sends the token as a query-string param instead of a header
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = ctx =>
        {
            var accessToken = ctx.Request.Query["access_token"];
            var path = ctx.HttpContext.Request.Path;

            if (!string.IsNullOrEmpty(accessToken) &&
                path.StartsWithSegments("/energyhub"))
            {
                ctx.Token = accessToken;
            }
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
        limiterOptions.QueueLimit = 2;
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
builder.Services.AddScoped<DataSeedingService>();

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
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins(
                  "https://localhost:7083",
                  "http://localhost:5167",
                  "http://localhost:3000")
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
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();
app.UseHttpsRedirection();

app.UseCors("AllowAll");

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<EnergyHub>("/energyhub");

app.Run();