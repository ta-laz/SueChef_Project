namespace SueChef.Models;

using Microsoft.EntityFrameworkCore;

public class SueChefDbContext : DbContext
{
    public DbSet<Chef>? Chefs { get; set; }
    public DbSet<Recipe>? Recipes { get; set; }
    public DbSet<Ingredient>? Ingredients { get; set; }
    public DbSet<RecipeIngredient>? RecipeIngredients { get; set; }
    public DbSet<User>? Users { get; set; }
    public DbSet<MealPlan>? MealPlans { get; set; }
    public DbSet<MealPlanRecipe>? MealPlanRecipes { get; set; }

    public SueChefDbContext(DbContextOptions<SueChefDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Chef → Recipe (one-to-many)
        modelBuilder.Entity<Chef>()
            .HasMany(c => c.Recipe)
            .WithOne(r => r.Chef)
            .HasForeignKey(r => r.ChefId)
            .OnDelete(DeleteBehavior.Cascade);

        // --- RecipeIngredient (many-to-many link between Recipe and Ingredient)
        modelBuilder.Entity<RecipeIngredient>(b =>
        {
            b.HasOne(ri => ri.Recipe)
            .WithMany(r => r.RecipeIngredients)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(ri => ri.Ingredient)
            .WithMany(i => i.RecipeIngredients)
            .HasForeignKey(ri => ri.IngredientId)
            .OnDelete(DeleteBehavior.Cascade);

            b.HasIndex(ri => new { ri.RecipeId, ri.IngredientId }).IsUnique();
        });

        // --- Recipe table configuration
        modelBuilder.Entity<Recipe>()
            .Property(r => r.Title)
            .HasMaxLength(200);

        modelBuilder.Entity<Recipe>()
            .Property(r => r.Category)
            .HasMaxLength(100);

        // --- Ingredient table configuration
        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Name)
            .HasMaxLength(100);

        modelBuilder.Entity<Ingredient>()
            .Property(i => i.Category)
            .HasMaxLength(100);

        // --- RecipeIngredient composite uniqueness
        modelBuilder.Entity<RecipeIngredient>()
            .HasIndex(ri => new { ri.RecipeId, ri.IngredientId })
            .IsUnique();
        
    }
}
