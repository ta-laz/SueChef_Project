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
            .Where(f => f.UserId == currentUserId && !f.IsDeleted)
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

    // [Route("/Favourites/{recipeId}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavourite(int recipeId)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

        // Check if a favourite record already exists (including soft-deleted)
        var favourite = await _db.Favourites
            .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == currentUserId);

        if (favourite != null)
        {
            // Toggle IsDeleted
            favourite.IsDeleted = !favourite.IsDeleted;

            await _db.SaveChangesAsync();

            TempData["Success"] = favourite.IsDeleted
                ? "Recipe removed from favourites."
                : "Recipe added to favourites.";
        }
        else
        {
            // Add new favourite
            _db.Favourites.Add(new Favourite
            {
                UserId = currentUserId,
                RecipeId = recipeId,
                IsDeleted = false
            });

            await _db.SaveChangesAsync();
            TempData["Success"] = "Recipe added to favourites.";
        }

        return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFavourite(int favouriteId)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

        var favourite = await _db.Favourites
            .FirstOrDefaultAsync(f => f.Id == favouriteId && f.UserId == currentUserId);

        if (favourite == null)
        {
            TempData["ErrorMessage"] = "Favourite not found.";
            return RedirectToAction("Index"); // Or wherever your All Favourites view is
        }

        favourite.IsDeleted = true;
        await _db.SaveChangesAsync();

        TempData["Success"] = "Recipe removed from favourites.";
        return RedirectToAction("Index");
    }


    // NONE OF THIS IS WORKING FMLLLLL
    // [Route("/Favourites/Delete/{id}")]
    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public async Task<IActionResult> DeleteFavourite(int id)
    // {
    //     int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

    //     var favourite = await _db.Favourites
    //         .Include(f => f.Recipe)
    //         .FirstOrDefaultAsync(f => f.Id == id && f.UserId == currentUserId && !f.IsDeleted);

    //     if (favourite == null)
    //     {
    //         TempData["ErrorMessage"] = "Favourite not found or already deleted.";
    //         return RedirectToAction("Index");
    //     }

    //     favourite.IsDeleted = true;
    //     await _db.SaveChangesAsync();

    //     TempData["DeletedFavouriteId"] = favourite.Id;
    //     TempData["DeletedRecipeName"] = favourite.Recipe.Title;
    //     TempData["SuccessMessage"] = $"Recipe '{favourite.Recipe.Title}' removed from favourites.";

    //     return RedirectToAction("Index");
    // }



}
