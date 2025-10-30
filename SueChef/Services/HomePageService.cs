using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Services;

public interface IHomePageService
{
    Task<HomePageViewModel> GetHomePageViewModelAsync(int? currentUserId);
}

public class HomePageService : IHomePageService
{
    private readonly SueChefDbContext _db;

    public HomePageService(SueChefDbContext db)
    {
        _db = db;
    }

    public async Task<HomePageViewModel> GetHomePageViewModelAsync(int? currentUserId)
    {
        // Load all recipes once
        var recipes = await _db.Recipes
            .Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                r.RecipePicturePath,
                r.Category,
                r.DifficultyLevel,
                r.IsVegetarian,
                r.IsDairyFree,
                r.PrepTime,
                r.CookTime
            })
            .ToListAsync();

        // Load ratings and favourites in bulk
        var ratings = await _db.Ratings
            .Where(rt => rt.Stars.HasValue)
            .GroupBy(rt => rt.RecipeId)
            .Select(g => new
            {
                RecipeId = g.Key,
                Count = g.Count(),
                Avg = g.Average(rt => rt.Stars.Value)
            })
            .ToDictionaryAsync(x => x.RecipeId);

        var favourites = currentUserId != null
    ? await _db.Favourites
        .Where(f => f.UserId == currentUserId && !f.IsDeleted && f.RecipeId.HasValue)
        .Select(f => f.RecipeId!.Value)   // safe because we checked HasValue
        .ToHashSetAsync()
    : new HashSet<int>();

        // Build RecipeCardViewModels
        var recipeCards = recipes.Select(r => new RecipeCardViewModel
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath,
            Category = r.Category,
            DifficultyLevel = r.DifficultyLevel,
            IsVegetarian = r.IsVegetarian,
            IsDairyFree = r.IsDairyFree,
            PrepTime = r.PrepTime,
            CookTime = r.CookTime,
            RatingCount = ratings.TryGetValue((int)r.Id, out var rate) ? rate.Count : 0,
            AverageRating = ratings.TryGetValue((int)r.Id, out var rate2) ? rate2.Avg : 0,
            IsFavourite = favourites.Contains((int)r.Id)
        }).ToList();

        // Featured recipes by ID (you can move these to config later)
        var featuredIds = new List<int> { 41, 47, 2 };
        var featuredRecipes = recipeCards.Where(r => featuredIds.Contains((int)r.Id)).ToList();

        var topFeatured = featuredRecipes.FirstOrDefault(r => r.Id == 41);
        var middleFeatured = featuredRecipes.FirstOrDefault(r => r.Id == 47);
        var bottomFeatured = featuredRecipes.FirstOrDefault(r => r.Id == 2);

        // Carousels
        var allCarousel = new RecipeCarouselViewModel
        {
            Title = "All Recipes",
            CarouselId = "allRecipesCarousel",
            Recipes = recipeCards.OrderBy(_ => Guid.NewGuid()).ToList()
        };

        var vegetarianCarousel = new RecipeCarouselViewModel
        {
            Title = "Vegetarian Recipes",
            CarouselId = "vegCarousel",
            Recipes = recipeCards.Where(r => r.IsVegetarian).Take(10).ToList()
        };

        var dairyFreeCarousel = new RecipeCarouselViewModel
        {
            Title = "Dairy-Free Recipes",
            CarouselId = "dairyFreeCarousel",
            Recipes = recipeCards.Where(r => r.IsDairyFree).Take(10).ToList()
        };

        var easyCarousel = new RecipeCarouselViewModel
        {
            Title = "For beginners",
            CarouselId = "easyCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 1).ToList()
        };

        var mediumCarousel = new RecipeCarouselViewModel
        {
            Title = "For those wanting a little challenge",
            CarouselId = "mediumCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 2).ToList()
        };

        var hardCarousel = new RecipeCarouselViewModel
        {
            Title = "For the real SueChefs!",
            CarouselId = "hardCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 3).ToList()
        };

        var quickCarousel = new RecipeCarouselViewModel
        {
            Title = "Quick Meals",
            CarouselId = "quickCarousel",
            Recipes = recipeCards.Where(r => r.PrepTime + r.CookTime < 60).ToList()
        };

        var highlyRatedCarousel = new RecipeCarouselViewModel
        {
            Title = "Highly Rated Recipes",
            CarouselId = "highlyRatedCarousel",
            Recipes = recipeCards.Where(r => r.AverageRating > 4)
                                 .OrderByDescending(r => r.AverageRating)
                                 .Take(10).ToList()
        };

        var popularCarousel = new RecipeCarouselViewModel
        {
            Title = "Most Popular Recipes",
            CarouselId = "mostPopularCarousel",
            Recipes = recipeCards.OrderByDescending(r => r.RatingCount)
                                 .Take(10).ToList()
        };

        // Categories
        var categoryCarousel = new CategoryCarouselViewModel
        {
            Categories = new List<CategoryCardViewModel>
            {
                BuildCategory(recipeCards, "Easy Recipes", "easy", r => r.DifficultyLevel == 1),
                BuildCategory(recipeCards, "Medium Recipes", "medium", r => r.DifficultyLevel == 2),
                BuildCategory(recipeCards, "Hard Recipes", "hard", r => r.DifficultyLevel == 3),
                BuildCategory(recipeCards, "Quick Recipes", "quick", r => r.PrepTime + r.CookTime < 60),
                BuildCategory(recipeCards, "Top 10 Recipes", "highlyrated", r => r.AverageRating > 4),
                BuildCategory(recipeCards, "Most Popular Recipes", "mostpopular", r => r.RatingCount > 5),
                BuildCategory(recipeCards, "Dairy-Free Recipes", "dairyfree", r => r.IsDairyFree),
                BuildCategory(recipeCards, "Vegetarian Recipes", "vegetarian", r => r.IsVegetarian)
            }.Where(c => c != null).ToList()
        };

        return new HomePageViewModel
        {
            RecipeCards = recipeCards,
            TopFeaturedRecipe = ToFeatured(topFeatured),
            MiddleFeaturedRecipe = ToFeatured(middleFeatured),
            BottomFeaturedRecipe = ToFeatured(bottomFeatured),
            AllRecipesCarousel = allCarousel,
            VegetarianRecipesCarousel = vegetarianCarousel,
            DairyFreeRecipesCarousel = dairyFreeCarousel,
            EasyRecipesCarousel = easyCarousel,
            MediumRecipesCarousel = mediumCarousel,
            HardRecipesCarousel = hardCarousel,
            QuickRecipesCarousel = quickCarousel,
            HighlyRatedRecipesCarousel = highlyRatedCarousel,
            MostPopularRecipesCarousel = popularCarousel,
            CategoryCarouselViewModel = categoryCarousel
        };
    }

    private static CategoryCardViewModel? BuildCategory(
        IEnumerable<RecipeCardViewModel> recipes,
        string text, string category, Func<RecipeCardViewModel, bool> predicate)
    {
        var recipe = recipes.FirstOrDefault(predicate);
        return recipe == null ? null : new CategoryCardViewModel
        {
            Id = (int)recipe.Id,
            Text = text,
            RecipePicturePath = recipe.RecipePicturePath,
            Category = category
        };
    }

    private static FeaturedRecipeViewModel? ToFeatured(RecipeCardViewModel? r) =>
        r == null ? null : new FeaturedRecipeViewModel
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath,
            Category = r.Category
        };
}
