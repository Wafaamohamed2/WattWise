using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Service.Services;
using EnergyOptimizer.Service.Services.Abstract;
using EnergyOptimizer.Service.Services.Implementation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyOptimizer.Service
{
    public static class ApplicationServiceRegistration
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<GeminiSettings>(configuration.GetSection("Gemini"));
            services.AddScoped<IGeminiService, GeminiService>();
            services.AddScoped<IPatternDetectionService, PatternDetectionService>();
            services.AddScoped<IAIAnalysisService, AIAnalysisService>();
            services.AddScoped<IDataCleanupService, DataCleanupService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IJwtTokenService, JwtTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            services.AddTransient<DataSeedingService>();

            return services;
        }
    }
}
