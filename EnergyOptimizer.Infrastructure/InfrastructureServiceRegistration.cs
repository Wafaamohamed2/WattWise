using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Infrastructure.Consumers;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Infrastructure.Repositories;
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

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContextFactory<EnergyDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            services.AddDbContext<EnergyDbContext>(options =>
                options.UseSqlServer(connectionString), ServiceLifetime.Scoped);

            services.AddMassTransit(x =>
            {
                x.AddConsumer<EnergyReadingConsumer>();

                x.UsingRabbitMq((context, cfg) =>
                {
                    cfg.Host("localhost", "/", h =>
                    {
                        h.Username("guest");
                        h.Password("guest");
                    });

                    cfg.ReceiveEndpoint("energy-readings-queue", e =>
                    {
                        e.ConfigureConsumer<EnergyReadingConsumer>(context);
                    });
                });
            });

            return services;
        }
    }
}
