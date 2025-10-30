namespace SueChef.Models;

using Microsoft.EntityFrameworkCore;

public class SueChefDbContext : DbContext
{
    public DbSet<Chef>? Chefs { get; set; } = null!;
    public DbSet<Recipe>? Recipes { get; set; } = null!;
    public DbSet<Ingredient>? Ingredients { get; set; } = null!;
    public DbSet<RecipeIngredient>? RecipeIngredients { get; set; } = null!;
    public DbSet<User>? Users { get; set; } = null!;
    public DbSet<MealPlan>? MealPlans { get; set; } = null!;
    public DbSet<MealPlanRecipe>? MealPlanRecipes { get; set; } = null!;
    public DbSet<Rating>? Ratings { get; set; } = null!;
    public DbSet<Favourite>? Favourites { get; set; } = null!;

    public DbSet<Comment>? Comments { get; set; }
    public DbSet<ShoppingList>? ShoppingLists { get; set; }

    public SueChefDbContext(DbContextOptions<SueChefDbContext> options) : base(options)
    {
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Update MealPlan UpdatedOn for direct modifications
        foreach (var entry in ChangeTracker.Entries<MealPlan>())
        {
            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                entry.Entity.UpdatedOn = DateTime.UtcNow;
            }
        }

        // Update MealPlan UpdatedOn if related MealPlanRecipe was added or deleted
        var mealPlanRecipeEntries = ChangeTracker.Entries<MealPlanRecipe>().ToList();

        foreach (var entry in mealPlanRecipeEntries)
        {
            if (entry.State == EntityState.Added || entry.State == EntityState.Modified ||  entry.State == EntityState.Deleted)
            {
                var mealPlan = await MealPlans.FindAsync(entry.Entity.MealPlanId);
                if (mealPlan != null)
                {
                    mealPlan.UpdatedOn = DateTime.UtcNow;
                    // Mark it as modified so EF updates it
                    Entry(mealPlan).State = EntityState.Modified;
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // --- Chef → Recipe (one-to-many)
        modelBuilder.Entity<Chef>()
            .HasMany(c => c.Recipes)
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

        // --- comment table 
        modelBuilder.Entity<Comment>(b =>
        {
            // Comment → User (many-to-one)
            b.HasOne(c => c.User)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);
            
            // Comment → Recipe (many-to-one)
            b.HasOne(c => c.Recipe)
            .WithMany(r => r.Comments)
            .HasForeignKey(c => c.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);
        });

        // --- rating table 
        modelBuilder.Entity<Rating>(r =>
        {
            r.HasOne(ra => ra.Recipe)
            .WithMany(r => r.Ratings)
            .HasForeignKey(ri => ri.RecipeId)
            .OnDelete(DeleteBehavior.Cascade);

            // Rating -> User (many-to-one)
            r.HasOne(r => r.User)
            .WithMany(u => u.Ratings)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

            r.HasIndex(r => new { r.UserId, r.RecipeId }).IsUnique();
        });
    
        
        // Sets servings to 4 as default in favourites table
        modelBuilder.Entity<Favourite>()
            .Property(f => f.Servings)
            .HasDefaultValue(4);


        
        
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
            b.Property(mp => mp.CreatedOn)
                .HasColumnType("timestamptz")  // PostgreSQL timestamp with timezone
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            b.Property(mp => mp.UpdatedOn)
                .HasColumnType("timestamptz")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
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

        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(100);

            b.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(200);

            // Relationships — cascade deletes ensure linked data is removed with the User

            b.HasMany(u => u.Favourites)
                .WithOne(f => f.User)
                .HasForeignKey(f => f.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.Comments)
                .WithOne(c => c.User)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.Ratings)
                .WithOne(r => r.User)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(u => u.MealPlans)
                .WithOne(mp => mp.User)
                .HasForeignKey(mp => mp.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

}

