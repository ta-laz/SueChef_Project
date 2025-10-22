using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using SueChef.Services;
using DotNetEnv;

namespace SueChef.Models
{
    public class SueChefDbContextFactory : IDesignTimeDbContextFactory<SueChefDbContext>
    {
        public SueChefDbContext CreateDbContext(string[] args)
        {
            var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
            if (File.Exists(envPath)) { Env.Load(envPath); }

            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cs = ConnectionStringResolver.ResolveNpgsql(config);

            var options = new DbContextOptionsBuilder<SueChefDbContext>()
                .UseNpgsql(cs)
                .Options;

            return new SueChefDbContext(options);
        }
    }
}