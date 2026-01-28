using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class EnergyDbContextFactory : IDesignTimeDbContextFactory<EnergyDbContext>
{
    public EnergyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EnergyDbContext>();
        optionsBuilder.UseSqlServer("Server=localhost;Database=EnergyOptimizerDB;Trusted_Connection=True;MultipleActiveResultSets=true"); 

        return new EnergyDbContext(optionsBuilder.Options);
    }
}
