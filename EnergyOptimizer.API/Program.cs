using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Infrastructure.Data;
using Serilog;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.API.Services;
using EnergyOptimizer.AI.Services;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

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
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<EnergyDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true; 
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Configure Rate Limiting
builder.Services.AddRateLimiter( Options =>
{
    // AI Analysis Rate Limiter
    Options.AddSlidingWindowLimiter("AI_Policy", config =>
    {
        config.PermitLimit = 10;
        config.Window = TimeSpan.FromMinutes(1);
        config.SegmentsPerWindow = 5;
        config.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        config.QueueLimit = 0;
    });

    // Concurrency Limiter for Heavy I/O Operations (Export CSV)
    Options.AddConcurrencyLimiter("HeavyIoPolicy", opt =>
     {
        opt.PermitLimit = 2; 
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2;
     });
      
    // General Rate Limiter for Dashboard & Browssing
    Options.AddTokenBucketLimiter("GeneralPolicy", opt =>
    {
        opt.TokenLimit = 50; 
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 10;
        opt.TokensPerPeriod = 25;
    });

    Options.OnRejected = async (context, token) =>
    {
        context.HttpContext.Response.StatusCode = 429; // Too Many Requests
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.", token);
    };
});


// Add Background Service
builder.Services.AddHostedService<EnergyReadingSimulatorService>();
builder.Services.AddHostedService<AlertDetectionService>();

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
builder.Services.AddScoped<PatternDetectionService>();

// Register Pattern Detection Service
builder.Services.AddScoped<AIAnalysisBackgroundService>();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.WithOrigins("http://localhost:3000", "http://localhost:5173") // React/Vite ports
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();
app.UseRateLimiter();
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();
app.MapHub<EnergyHub>("/energyhub");

app.Run();