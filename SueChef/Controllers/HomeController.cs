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

    public async Task<IActionResult> Index()
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
            Recipes = recipeCards.ToList()
        };

        var vegetarianRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Vegetarian Recipes",
            CarouselId = "vegCarousel",
            Recipes = recipeCards.Where(r => r.IsVegetarian).Skip(5).ToList()
        };

        var dairyFreeRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Dairy-Free Recipes",
            CarouselId = "dairyFreeCarousel",
            Recipes = recipeCards.Where(r => r.IsDairyFree).ToList()
        };

        var easyRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For beginners",
            CarouselId = "easyCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 1).ToList()
        };

        var mediumRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For those wanting a little challenge",
            CarouselId = "mediumCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 2).ToList()
        };

        var hardRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "For the real SueChefs!",
            CarouselId = "hardCarousel",
            Recipes = recipeCards.Where(r => r.DifficultyLevel == 3).ToList()
        };

        var quickRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Quick Meals",
            CarouselId = "quickCarousel",
            Recipes = recipeCards.Where(r => r.PrepTime + r.CookTime < 60).ToList()
        };

        var highlyRatedRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Highly Rated Recipes",
            CarouselId = "highlyRatedCarousel",
            Recipes = recipeCards
            .OrderByDescending(r => r.AverageRating)
            .Where(r => r.AverageRating > 4).ToList()
        };

        var mostRatedRecipesCarousel = new RecipeCarouselViewModel
        {
            Title = "Most Rated Recipes",
            CarouselId = "mostRatedCarousel",
            Recipes = recipeCards
            .OrderByDescending(r => r.RatingCount)
            .Take(10)
            .ToList()
        };

        // these make new CategoryCardViewModels that are made based on selection criteria from the recipes
        // each one is added to a list inside a CategoryCarouselViewModel

        var easyCategory = await _db.Recipes
        .Where(r => r.DifficultyLevel == 1 && r.Id != 3)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "New to cooking? Try these!",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var mediumCategory = await _db.Recipes
        .Where(r => r.DifficultyLevel == 2)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "For those that want a bit more of a challenge",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var hardCategory = await _db.Recipes
        .Where(r => r.DifficultyLevel == 3)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "SueChef herself would find these tricky",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var quickCategory = await _db.Recipes
        .Where(r => r.PrepTime + r.CookTime < 60 && r.Id != 1)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "Short on time? You can whip these meals up in a jiffy",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var highestRatedRecipe = recipeCards
        .OrderByDescending(r => r.AverageRating)
        .FirstOrDefault();

        int highestRatedRecipeId = highestRatedRecipe.Id;

        var highlyRatedCategory = await _db.Recipes
        .Where(r => r.Id == highestRatedRecipeId)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "These are our most highly rated recipes",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var mostRatedRecipe = recipeCards
        .OrderByDescending(r => r.RatingCount)
        .FirstOrDefault();

        int mostRatedRecipeId = mostRatedRecipe.Id;

        var mostRatedCategory = await _db.Recipes
        .Where(r => r.Id == mostRatedRecipeId)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "Our users can't stop rating these recipes",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var dairyFreeCategory = await _db.Recipes
        .Where(r => r.IsDairyFree == true && r.Id != 1 && r.Id != 3 && r.Id != 6)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "No dairy? No problem!",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var vegetarianCategory = await _db.Recipes
        .Where(r => r.IsVegetarian == true && r.Id != 1 && r.Id != 3 && r.Id != 6)
        .Select(r => new CategoryCardViewModel
        {
            Id = r.Id,
            Text = "There's no meat in sight, we promise",
            RecipePicturePath = r.RecipePicturePath
        })
        .FirstOrDefaultAsync();

        var categoryCarouselViewModel = new CategoryCarouselViewModel
        {
            Categories = new List<CategoryCardViewModel> { easyCategory, mediumCategory, hardCategory, quickCategory, highlyRatedCategory, mostRatedCategory, dairyFreeCategory, vegetarianCategory }
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
            HighlyRatedRecipesCarousel = highlyRatedRecipesCarousel,
            MostRatedRecipesCarousel = mostRatedRecipesCarousel,
            CategoryCarouselViewModel = categoryCarouselViewModel
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
