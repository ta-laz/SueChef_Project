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

}
