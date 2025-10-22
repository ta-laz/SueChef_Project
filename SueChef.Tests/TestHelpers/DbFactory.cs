using DotNetEnv;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SueChef.Models;
using SueChef.Services;

namespace SueChef.TestHelpers;

public static class DbFactory
{
    static DbFactory()
    {
        // Try a few likely locations for the app's .env:
        var candidates = new[]
        {
            Path.Combine(Directory.GetCurrentDirectory(), ".env"),                                   // if you keep a test .env
            Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "SueChef", ".env")), // typical when tests run in SueChef.Tests
            Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SueChef", ".env"))
        };

        foreach (var p in candidates)
        {
            if (File.Exists(p))
            {
                Env.Load(p);
                Console.WriteLine($"📦 Tests loaded environment from: {p}");
                break;
            }
        }
    }

    public static SueChefDbContext Create()
    {
        var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

        var cs = ConnectionStringResolver.ResolveNpgsql(config);
        var opts = new DbContextOptionsBuilder<SueChefDbContext>()
            .UseNpgsql(cs)
            .Options;

        return new SueChefDbContext(opts);
    }
}