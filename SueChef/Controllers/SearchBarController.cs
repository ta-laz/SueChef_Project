using Microsoft.AspNetCore.Mvc;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace SueChef.Controllers;

public class SearchBarController : Controller
{
    private readonly ILogger<UsersController> _logger;
    private readonly SueChefDbContext _db;

    public SearchBarController(ILogger<UsersController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }
    
    [HttpGet("/search")]
    public async Task<IActionResult> Index(string SearchQuery)
    {
        var recipeCards = await _db.Recipes
            .OrderBy(r => Guid.NewGuid())
            .Select(r => new RecipeCardViewModel
            {
                Id = r.Id,
                Title = r.Title ?? "Untitled Recipe",
                Description = r.Description,
                RecipePicturePath = r.RecipePicturePath ?? "/images/bolognese.png",
                Category = r.Category ?? "Uncategorized",
                DifficultyLevel = r.DifficultyLevel,
                IsVegetarian = r.IsVegetarian,
                IsDairyFree = r.IsDairyFree
            })
            .ToListAsync();

        return View(recipeCards);
    }
}