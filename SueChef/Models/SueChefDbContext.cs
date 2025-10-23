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
    public DbSet<Rating>? Ratings { get; set; }

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

        // --- rating table 
        modelBuilder.Entity<Rating>(r =>
        {
            r.HasOne(ra => ra.Recipe)
            .WithMany(r => r.Ratings)
            .HasForeignKey(ri => ri.RecipeId)
            .HasForeignKey(ri => ri.UserId);
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

        // --- User → MealPlan (one-to-many)
        modelBuilder.Entity<MealPlan>(b =>
        {
            b.Property(mp => mp.MealPlanTitle).HasMaxLength(200);
            b.Property(mp => mp.CreatedOn).HasDefaultValueSql("CURRENT_DATE");
            b.Property(mp => mp.UpdatedOn).HasDefaultValueSql("CURRENT_DATE");
            b.HasOne(mp => mp.User)
             .WithMany(u => u.MealPlans)             // rename property on User
             .HasForeignKey(mp => mp.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- MealPlanRecipe (join)
        modelBuilder.Entity<MealPlanRecipe>(b =>
        {
            b.HasOne(mpr => mpr.MealPlan)
             .WithMany(mp => mp.MealPlanRecipes)
             .HasForeignKey(mpr => mpr.MealPlanId)
             .OnDelete(DeleteBehavior.Cascade);

            b.HasOne(mpr => mpr.Recipe)
             .WithMany(r => r.MealPlanRecipes)
             .HasForeignKey(mpr => mpr.RecipeId)
             .OnDelete(DeleteBehavior.Restrict);

            b.HasIndex(mpr => new { mpr.MealPlanId, mpr.RecipeId }).IsUnique();
        });

    }
}
