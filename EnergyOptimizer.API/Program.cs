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
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Explicitly load appsettings.json (ensures Gemini section is available)
builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();


// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/energy-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var errors = actionContext.ModelState
            .Where(e => e.Value.Errors.Count > 0)
            .SelectMany(x => x.Value.Errors)
            .Select(x => x.ErrorMessage).ToArray();

        return new BadRequestObjectResult(new ApiResponse(400, "Validation Failed", errors));
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

// Add DbContext
builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Identity Services
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<EnergyDbContext>()
    .AddDefaultTokenProviders();

// JWT Authentication
var jwtSettings = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(options => {
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]!))
    };
});

// Add MediatR and register handlers from the Core assembly
builder.Services.AddMediatR(cfg => {
    cfg.RegisterServicesFromAssembly(typeof(RunGlobalAnalysisCommand).Assembly);
});

// Add Background Service
builder.Services.AddHostedService<EnergyReadingSimulatorService>();
builder.Services.AddHostedService<AlertDetectionService>();

// Add Energy Hub Service
builder.Services.AddScoped<IEnergyHubService, EnergyHubService>();

// Add Data Seeding Service
builder.Services.AddTransient<DataSeedingService>();

// Configure Gemini Settings
builder.Services.Configure<GeminiSettings>(
    builder.Configuration.GetSection("Gemini"));

// Add Memory Cache for AI responses
builder.Services.AddMemoryCache();

// Register HttpClient for Gemini Service
builder.Services.AddHttpClient();

// Register Gemini Service
builder.Services.AddScoped<IGeminiService, GeminiService>();

// Register Pattern Detection Service
builder.Services.AddScoped<IPatternDetectionService, PatternDetectionService>();

// Register AI Analysis and Data Cleanup Services
builder.Services.AddScoped<IAIAnalysisService, AIAnalysisService>();
builder.Services.AddScoped<IDataCleanupService, DataCleanupService>();
builder.Services.AddHostedService<AIAnalysisBackgroundService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.WithOrigins("https://localhost:7083", "http://localhost:5167", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();

// add custom exception middleware
app.UseMiddleware<ExceptionMiddleware>();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EnergyHub>("/energyhub");

app.Run();