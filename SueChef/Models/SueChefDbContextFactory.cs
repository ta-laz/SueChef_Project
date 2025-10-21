using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SueChef.Models
{
    public class SueChefDbContextFactory : IDesignTimeDbContextFactory<SueChefDbContext>
    {
        public SueChefDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<SueChefDbContext>();

            // Replace with your actual connection string
            optionsBuilder.UseNpgsql("Host=localhost;Database=suechef_test;Username=postgres;Password=yourpassword");

            return new SueChefDbContext(optionsBuilder.Options);
        }
    }
}
