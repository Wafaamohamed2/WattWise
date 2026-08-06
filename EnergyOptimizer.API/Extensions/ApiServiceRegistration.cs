using System.Text;
using System.Threading.RateLimiting;
using EnergyOptimizer.API.Services;
using EnergyOptimizer.API.Swagger;
using EnergyOptimizer.Data.Identity;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EnergyOptimizer.API.Extensions
{
    public static class ApiServiceRegistration
    {
        public static IServiceCollection AddApiVersioningAndSwagger(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();

            services.AddApiVersioning(options =>
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

            services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
            services.AddSwaggerGen();

            return services;
        }

        public static IServiceCollection AddIdentityAndJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<EnergyDbContext>()
                .AddDefaultTokenProviders();

            var jwtSettings = configuration.GetSection("Jwt");
            var keyStr = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Key is missing.");

            services.AddAuthentication(options =>
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
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyStr))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = ctx =>
                    {
                        if (ctx.Request.Cookies.TryGetValue("access_token", out var cookieToken)
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            ctx.Token = cookieToken;
                            return Task.CompletedTask;
                        }

                        var authHeader = ctx.Request.Headers["Authorization"].FirstOrDefault();
                        if (authHeader?.StartsWith("Bearer ") == true)
                        {
                            ctx.Token = authHeader["Bearer ".Length..];
                            return Task.CompletedTask;
                        }

                        var accessToken = ctx.Request.Query["access_token"];
                        var path = ctx.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/notifications"))
                        {
                            ctx.Token = accessToken;
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddApiRateLimiting(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("auth", limiterOptions =>
                {
                    limiterOptions.Window = TimeSpan.FromMinutes(1);
                    limiterOptions.PermitLimit = 10;
                    limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    limiterOptions.QueueLimit = 0;
                });

                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            });

            return services;
        }

        public static IServiceCollection AddAppCaching(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddMemoryCache();

            var redisConnectionString = configuration.GetConnectionString("RedisConnection");
            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                services.AddStackExchangeRedisCache(options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "WattWise_";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

            return services;
        }

        public static IServiceCollection AddApiCors(this IServiceCollection services, IConfiguration configuration)
        {
            var allowedOrigins = configuration
                .GetSection("AllowedOrigins")
                .Get<string[]>()
                ?? new[] { "http://localhost:3000", "http://localhost:4200" };

            services.AddCors(options =>
            {
                options.AddPolicy("EnergyOptimizerCorsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            return services;
        }

        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {
            services.AddHostedService<EnergyReadingSimulatorService>();
            services.AddHostedService<AlertDetectionService>();
            services.AddHostedService<AIAnalysisBackgroundService>();
            services.AddHostedService<RefreshTokenCleanupService>();

            return services;
        }
    }
}
