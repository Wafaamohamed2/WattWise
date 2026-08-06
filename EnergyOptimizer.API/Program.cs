using EnergyOptimizer.API.Extensions;
using EnergyOptimizer.API.Hubs;
using EnergyOptimizer.API.Helpers;
using EnergyOptimizer.API.Middleware;
using EnergyOptimizer.API.Services;
using EnergyOptimizer.API.WebServices;
using EnergyOptimizer.Core;
using EnergyOptimizer.Core.Contracts;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure;
using EnergyOptimizer.Service;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Mvc;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();

// Logging (Serilog)
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/energy-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

// Controllers & Custom Model Validation
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

// Modular Architecture Registrations
builder.Services.AddApiVersioningAndSwagger();
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddCoreServices();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityAndJwtAuthentication(builder.Configuration);
builder.Services.AddApiRateLimiting();
builder.Services.AddAppCaching(builder.Configuration);
builder.Services.AddApiCors(builder.Configuration);

// Application & Infrastructure Custom Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, EnergyOptimizer.Infrastructure.Services.CurrentUserService>();
builder.Services.AddScoped<IEnergyHubService, EnergyHubService>();
builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.KeepAliveInterval = TimeSpan.FromSeconds(15);
    options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
});

// Options Configuration
builder.Services.Configure<EnergyReadingSimulatorService.SimulationOptions>(
    builder.Configuration.GetSection(EnergyReadingSimulatorService.SimulationOptions.SectionName));
builder.Services.Configure<AIAnalysisOptions>(
    builder.Configuration.GetSection(AIAnalysisOptions.SectionName));

// Background Hosted Services
builder.Services.AddBackgroundServices();

// AutoMapper & HttpClient
builder.Services.AddAutoMapper(typeof(MappingProfiles));
builder.Services.AddHttpClient();

var app = builder.Build();

app.UseSerilogRequestLogging();
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

// Rewrite non-versioned API requests (/api/xyz -> /api/v1/xyz)
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

app.UseCors("EnergyOptimizerCorsPolicy");

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<EnergyHub>("/energyhub");
app.MapHub<NotificationHub>("/hubs/notifications");

// Database Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var seedingService = services.GetRequiredService<DataSeedingService>();
        await seedingService.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();