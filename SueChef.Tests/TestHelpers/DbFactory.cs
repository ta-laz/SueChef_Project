using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using System.IO;

namespace SueChef.TestHelpers
{
    public static class DbFactory
    {
        public static SueChefDbContext CreateTestDb()
        {
            var options = new DbContextOptionsBuilder<SueChefDbContext>()
                .UseNpgsql("Host=localhost;Database=suechef_test;Username=postgres;Password=yourpassword") 
                .Options;

            return new SueChefDbContext(options);
        }
    }
}