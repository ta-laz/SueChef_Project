using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;


namespace SueChef.Controllers;

// [ServiceFilter(typeof(AuthenticationFilter))]
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
            .Where(f => f.UserId == currentUserId && !f.IsDeleted)
            .OrderByDescending(f => f.Id)
            .Select(f => new RecipeCardViewModel
            {
                Id = f.RecipeId,
                Title = f.Recipe.Title,
                Description = f.Recipe.Description,
                DifficultyLevel = f.Recipe.DifficultyLevel,
                IsVegetarian = f.Recipe.IsVegetarian,
                IsDairyFree = f.Recipe.IsDairyFree,
                RecipePicturePath = f.Recipe.RecipePicturePath,
                Category = f.Recipe.Category,
                MealPlanRecipeId = f.Id,
                PrepTime = f.Recipe.PrepTime,
                CookTime = f.Recipe.CookTime,
                RatingCount = _db.Ratings.Count(rt => rt.RecipeId == f.RecipeId && rt.Stars.HasValue),
                AverageRating = _db.Ratings
                    .Where(rt => rt.RecipeId == f.RecipeId && rt.Stars.HasValue)
                    .Average(rt => (double?)rt.Stars) ?? 0,
            })
            .ToListAsync();

        var FavouritesPageViewModel = new FavouritesPageViewModel
        {
            Favourites = allFavourites
        };

        return View(FavouritesPageViewModel);
    }


    // Add recipe to a favourite FROM INDIVIDUAL RECIPE PAGE (no AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleFavourite(int recipeId)
    {
        var currentUserId = HttpContext.Session.GetInt32("user_id");

        // If the user is not signed in:
        if (currentUserId == null)
        {
            TempData["ErrorMessage"] = "You must be signed in to favourite recipes.";
        }
        // If the user is signed in:
        // Check if a recipe already exists in favourites table (including soft-deleted)
        var favourite = await _db.Favourites
            .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == currentUserId);
        if (favourite != null)
        {
            // If already favourited, Toggle IsDeleted to un-favourite
            favourite.IsDeleted = !favourite.IsDeleted;
            await _db.SaveChangesAsync();
        }
        else
        {
            // Add recipe to favourites table
            _db.Favourites.Add(new Favourite
            {
                UserId = currentUserId,
                RecipeId = recipeId,
                IsDeleted = false
            });
            await _db.SaveChangesAsync();
        }
        return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
    }

    // Delete favourite - used in Favourites page button
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteFavourite(int favouriteId)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

        var favourite = await _db.Favourites
            .Include(f => f.Recipe)
            .FirstOrDefaultAsync(f => f.Id == favouriteId && f.UserId == currentUserId);

        if (favourite == null)
        {
            TempData["ErrorMessage"] = "Favourite not found.";
            return RedirectToAction("Index"); 
        }

        favourite.IsDeleted = true;
        await _db.SaveChangesAsync();

        // Store info in TempData for success message + undo
        TempData["DeletedRecipeId"] = favourite.Recipe.Id;
        TempData["DeletedRecipeName"] = favourite.Recipe.Title;
        TempData["Success"] = " removed from favourites.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> UndoDeleteFavourite(int id)
    {
        var recipe = await _db.Favourites
            .Include(f => f.Recipe)
            .FirstOrDefaultAsync(f => f.RecipeId == id && f.IsDeleted);
        if (recipe == null)
        {
            TempData["ErrorMessage"] = "Unable to undo deletion.";
            return RedirectToAction("Index");
        }
        recipe.IsDeleted = false;
        await _db.SaveChangesAsync();
        TempData["Success"] = $"Recipe: {recipe.Recipe.Title} restored successfully!";
        return RedirectToAction("Index");
    }

    // Method to add recipe to favourites FROM HOMEPAGE (AJAX)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Favourites/ToggleAjax")]
    public async Task<IActionResult> ToggleFavouriteAjax([FromForm] int recipeId)
    {
        var currentUserId = HttpContext.Session.GetInt32("user_id");

        if (currentUserId == null)
        {
            return Unauthorized(new
            {
                success = false,
                message = "You must be signed in to favourite recipes."
            });
        }

        var favourite = await _db.Favourites
            .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == currentUserId);

        if (favourite != null)
        {
            favourite.IsDeleted = !favourite.IsDeleted;
            await _db.SaveChangesAsync();

            return Ok(new
            {
                success = true,
                isFavourite = !favourite.IsDeleted,
            });
        }

        _db.Favourites.Add(new Favourite
    {
        UserId = currentUserId,
        RecipeId = recipeId,
        IsDeleted = false
    });
    await _db.SaveChangesAsync();

    return Ok(new { success = true, isFavourite = true });
    }

}
