using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EnergyOptimizer.Data
{
    public class EnergyDbContextFactory : IDesignTimeDbContextFactory<EnergyDbContext>
    {
        public EnergyDbContext CreateDbContext(string[] args)
        {
            var basePath = Directory.GetCurrentDirectory();
            var apiPath = Path.Combine(basePath, "../EnergyOptimizer.API");
            var targetPath = Directory.Exists(apiPath) ? apiPath : basePath;

            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(targetPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();

            var config = configBuilder.Build();
            var connectionString = config.GetConnectionString("DefaultConnection");

            var optionsBuilder = new DbContextOptionsBuilder<EnergyDbContext>();
            optionsBuilder.UseSqlServer("Server=localhost;Database=EnergyOptimizerDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True");

            return new EnergyDbContext(optionsBuilder.Options);
        }
    }
}
