using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly SueChefDbContext _db;

    public HomeController(ILogger<HomeController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    public async Task<IActionResult> Index(int count = 0)
    {

        // Only retrieve the data you want from the Recipes table and convert them into RecipeCardViewModel Objects:
        var recipeCards = await _db.Recipes
        .OrderBy(r => Guid.NewGuid())
        .Select(r => new RecipeCardViewModel
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

            RatingCount = _db.Ratings.Count(rt => rt.RecipeId == r.Id && rt.Stars.HasValue),
            AverageRating = _db.Ratings
            .Where(rt => rt.RecipeId == r.Id && rt.Stars.HasValue)
            .Average(rt => (double?)rt.Stars) ?? 0
        })
        .ToListAsync();

        // Only retrieve the data you want from the Recipes table and convert them into 1 FeaturedRecipeViewModel Object:
        var topFeaturedRecipe = await _db.Recipes
            .Where(r => r.Id == 41)
            .Select(r => new FeaturedRecipeViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                RecipePicturePath = r.RecipePicturePath,
                Category = r.Category
            })
            .FirstOrDefaultAsync();

        var middleFeaturedRecipe = await _db.Recipes
            .Where(r => r.Id == 47)
            .Select(r => new FeaturedRecipeViewModel
            {
                Id = r.Id,
                Title = r.Title,
                Description = r.Description,
                RecipePicturePath = r.RecipePicturePath,
                Category = r.Category
            })
            .FirstOrDefaultAsync();

        var bottomFeaturedRecipe = await _db.Recipes
        .Where(r => r.Id == 2)
        .Select(r => new FeaturedRecipeViewModel
        {
            Id = r.Id,
            Title = r.Title,
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath,
            Category = r.Category
        })
        .FirstOrDefaultAsync();

        var allRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "All Recipes",
            CarouselId = "allRecipesCarousel",
            Recipes = recipeCards.Skip(5).ToList()
        };

        var vegetarianRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Vegetarian Meals",
            CarouselId = "vegCarousel", // Unique ID
                                        // Filter the existing recipeCards list where IsVegetarian is true
            Recipes = recipeCards.Where(r => r.IsVegetarian).Skip(5).ToList()
        };

        var dairyFreeRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Dairy-Free Meals",
            CarouselId = "dairyFreeCarousel", // Unique ID
                                              // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.IsDairyFree).ToList()
        };

        var easyRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For beginners",
            CarouselId = "easyCarousel", // Unique ID
                                         // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 1).ToList()
        };

        var mediumRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For those wanting a little challenge",
            CarouselId = "mediumCarousel", // Unique ID
                                           // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 2).ToList()
        };

        var hardRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For the real SueChefs!",
            CarouselId = "hardCarousel", // Unique ID
                                         // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 3).ToList()
        };

        var quickRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Quick Meals",
            CarouselId = "quickCarousel", // Unique ID
                                          // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.PrepTime + r.CookTime < 60).ToList()
        };

        var highlyRatedRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Highly Rated Recipes",
            CarouselId = "highlyRatedCarousel", // Unique ID
                                                // Filter the existing recipeCards list where IsDairyFree is true
            Recipes = recipeCards.Where(r => r.AverageRating > 4).ToList()
        };

        // Combine the view models made above into a new HomePageViewModel object, this will get passed to the View:
        var AllViewModels = new HomePageViewModel
        {
            RecipeCards = recipeCards,
            TopFeaturedRecipe = topFeaturedRecipe,
            MiddleFeaturedRecipe = middleFeaturedRecipe,
            BottomFeaturedRecipe = bottomFeaturedRecipe,
            AllRecipesCarousel = allRecipesCarousel,
            VegetarianRecipesCarousel = vegetarianRecipesCarousel,
            DairyFreeRecipesCarousel = dairyFreeRecipesCarousel,
            EasyRecipesCarousel = easyRecipesCarousel,
            MediumRecipesCarousel = mediumRecipesCarousel,
            HardRecipesCarousel = hardRecipesCarousel,
            QuickRecipesCarousel = quickRecipesCarousel,
            HighlyRatedRecipesCarousel = highlyRatedRecipesCarousel
        };

        // Pass the list of view models into the View for this controller action
        return View(AllViewModels);
    }


    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
