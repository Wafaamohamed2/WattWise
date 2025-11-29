using EnergyOptimizer.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class EnergyDbContextFactory : IDesignTimeDbContextFactory<EnergyDbContext>
{
    public EnergyDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<EnergyDbContext>();
        optionsBuilder.UseSqlServer("Data Source=WAFAA;Initial Catalog=EnergyOptimizer;Integrated Security=True;Connect Timeout=30;Encrypt=True;Trust Server Certificate=True;Application Intent=ReadWrite;Multi Subnet Failover=False"); 

        return new EnergyDbContext(optionsBuilder.Options);
    }
}
