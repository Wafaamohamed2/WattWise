using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Consumers;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Infrastructure.Repositories;
using EnergyOptimizer.Infrastructure.Services;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnergyOptimizer.Infrastructure
{
    public static class InfrastructureServiceRegistration
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IIdentityService, IdentityService>();

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextFactory<EnergyDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            services.AddDbContext<EnergyDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            var rabbitHost = configuration["RabbitMQ:Host"] ?? "localhost";
            var rabbitUser = configuration["RabbitMQ:Username"] ?? "guest";
            var rabbitPass = configuration["RabbitMQ:Password"] ?? "guest";
            var useInMemory = bool.TryParse(configuration["RabbitMQ:UseInMemoryFallback"], out var result) && result;

            services.AddMassTransit(x =>
            {
                x.AddConsumer<EnergyReadingConsumer>();

                if (useInMemory)
                {
                    x.UsingInMemory((context, cfg) =>
                    {
                        cfg.ReceiveEndpoint("energy-readings-queue", e =>
                        {
                            e.ConfigureConsumer<EnergyReadingConsumer>(context);
                        });
                    });
                }
                else
                {
                    x.UsingRabbitMq((context, cfg) =>
                    {
                        cfg.Host(rabbitHost, "/", h =>
                        {
                            h.Username(rabbitUser);
                            h.Password(rabbitPass);
                        });

                        cfg.ReceiveEndpoint("energy-readings-queue", e =>
                        {
                            e.ConfigureConsumer<EnergyReadingConsumer>(context);
                        });
                    });
                }
            });

            return services;
        }
    }
}
