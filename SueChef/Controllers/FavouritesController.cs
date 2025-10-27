using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;


namespace SueChef.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class FavouritesController : Controller
{
    private readonly ILogger<FavouritesController> _logger;
    private readonly SueChefDbContext _db;

    public FavouritesController(ILogger<FavouritesController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }


    [Route("/Favourites")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var allFavourites = await _db.Favourites
            .Include(f => f.Recipe)
            .Where(f => f.UserId == currentUserId)
            .OrderByDescending(f => f.Id)
            .Select(f => new FavouritesViewModel
            {
                Id = f.Id,
                UserId = f.UserId,
                RecipeId = f.RecipeId,
                Servings = f.Servings,
                Recipe = f.Recipe
            })
            .ToListAsync();

        var FavouritesPageViewModel = new FavouritesPageViewModel
        {
            Favourites = allFavourites
        };

        return View(FavouritesPageViewModel);
    }

    [Route("/Favourites/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecipe(int recipeId)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        bool exists = await _db.Favourites.AnyAsync(f => f.RecipeId == recipeId && f.UserId == currentUserId);
        if (exists)
        {
            // Show an error message in TempData
            TempData["ErrorMessage"] = $"This recipe is already in Favourites.";
            return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
        }
        _db.Favourites.Add(new Favourite
        {
            UserId = currentUserId,
            RecipeId = recipeId
        });
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Recipe added to Favourites";
        return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
    }


}
