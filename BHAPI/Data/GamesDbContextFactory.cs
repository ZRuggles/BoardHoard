using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace BoardHoardApi.Data;

public class GamesDbContextFactory : IDesignTimeDbContextFactory<GamesDbContext>
{
    public GamesDbContext CreateDbContext(string[] args)
    {
        // Resolve config relative to the API project directory
        var cfg = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var cs = cfg.GetConnectionString("GamesDb")
            ?? throw new InvalidOperationException("Missing connection string 'GamesDb'.");

        var opts = new DbContextOptionsBuilder<GamesDbContext>()
            .UseSqlServer(cs)
            .Options;

        return new GamesDbContext(opts);
    }
}
