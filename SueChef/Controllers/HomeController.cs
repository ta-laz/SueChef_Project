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

    public async Task<IActionResult> Index2()
    {
        var recipes = await _db.Recipes
        .Include(r => r.RecipeIngredients)
            .ThenInclude(ri => ri.Ingredient)
        .ToListAsync();
        ViewBag.Recipes = recipes;
        return View();
    }

    public async Task<IActionResult> Index()
    {
        var recipes = new List<RecipeCardViewModel>
        {
            new RecipeCardViewModel
            {
                Id = 1,
                Title = "Classic Lasagna",
                ImageUrl = "/images/bolognese.png",
                ShortDescription = "A comforting Italian classic layered with rich meat sauce."
            },
            new RecipeCardViewModel
            {
                Id = 2,
                Title = "Chocolate Brownies",
                ImageUrl = "/images/bolognese.png",
                ShortDescription = "Deliciously fudgy brownies with a crispy top."
            }
        };

        return View(recipes);
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
