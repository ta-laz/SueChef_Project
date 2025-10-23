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
            Title = r.Title ?? "Untitled Recipe", // The ?? is a 'null coalescing operator', means that if there is no title, use "Untitled Recipe"
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath ?? "/images/bolognese.png",
            Category = r.Category ?? "Uncategorized",
            DifficultyLevel = r.DifficultyLevel,
            IsVegetarian = r.IsVegetarian,
            IsDairyFree = r.IsDairyFree
        })
        .ToListAsync();

        // Only retrieve the data you want from the Recipes table and convert them into 1 FeaturedRecipeViewModel Object:
        var featuredRecipe = await _db.Recipes
            .Where(r => r.Id == 3)
            .Select(r => new FeaturedRecipeViewModel
        {
            Id = r.Id,
            Title = r.Title ?? "Untitled Recipe", // The ?? is a 'null coalescing operator', means that if there is no title, use "Untitled Recipe"
            Description = r.Description,
            RecipePicturePath = r.RecipePicturePath ?? "/images/bolognese.png",
            Category = r.Category ?? "Uncategorized"
        })
            .FirstOrDefaultAsync();

        // Combine the view models made above into a new HomePageViewModel object, this will get passed to the View:
        var AllViewModels = new HomePageViewModel
        {
            RecipeCards = recipeCards,
            FeaturedRecipe = featuredRecipe
        };

        // Pass the list of view models into the View for this controller action
        return View(AllViewModels);
    }

    // OLD CODE:
    // public async Task<IActionResult> Index()
    // {
    //     var recipes = new List<RecipeCardViewModel>
    //     {
    //         new RecipeCardViewModel
    //         {
    //             Id = 1,
    //             Title = "Classic Lasagna",
    //             ImageUrl = "/images/bolognese.png",
    //             ShortDescription = "A comforting Italian classic layered with rich meat sauce."
    //         },
    //         new RecipeCardViewModel
    //         {
    //             Id = 2,
    //             Title = "Chocolate Brownies",
    //             ImageUrl = "/images/bolognese.png",
    //             ShortDescription = "Deliciously fudgy brownies with a crispy top."
    //         }
    //     };

    //     return View(recipes);
    // }


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
