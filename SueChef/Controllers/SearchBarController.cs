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
    public async Task<IActionResult> Index(string? searchQuery, string? category, string? chef, List<string>? ingredients, string? dietary, int? difficulty, string? duration)
    {
        // Populating the drop downs here from the database
        var allIngredients = await _db.Ingredients
            .OrderBy(i => i.Name)
            .Select(i => i.Name!)
            .ToListAsync();

        var allCategories = await _db.Recipes
            .Select(r => r.Category)
            .Where(c => c != null)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        var allChefs = await _db.Chefs
            .OrderBy(c => c.Name)
            .Select(c => c.Name!)
            .ToListAsync();

        // Check whether anything has actually been searched otherwise no results appear
        bool hasSearch = !string.IsNullOrWhiteSpace(searchQuery)
             || !string.IsNullOrWhiteSpace(category)
             || !string.IsNullOrWhiteSpace(chef)
             || (ingredients != null && ingredients.Any());

        // Set up the recipeCards outside the if statement cause it will break the view otherwise
        var recipeCards = new List<RecipeCardViewModel>();

        // Check if anything has been searched and then run through the logic of bringing the right bits out 
        if (hasSearch)
        {
            var query = _db.Recipes
                .Include(r => r.Chef)
                .Include(r => r.RecipeIngredients)
                    .ThenInclude(ri => ri.Ingredient)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                query = query.Where(r =>
                    (r.Title != null && EF.Functions.ILike(r.Title, $"%{searchQuery}%")) ||
                    (r.Description != null && EF.Functions.ILike(r.Description, $"%{searchQuery}%")) ||
                    (r.Chef != null && EF.Functions.ILike(r.Chef.Name!, $"%{searchQuery}%")) ||
                    r.RecipeIngredients.Any(ri => EF.Functions.ILike(ri.Ingredient.Name!, $"%{searchQuery}%"))
                );
            }

            // Category filter 
            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(r => r.Category == category);

            // Ingredient checkboxes (only if all ingredients are in there)
            if (ingredients != null && ingredients.Any())
                query = query.Where(r => ingredients.All(selected =>
                        r.RecipeIngredients.Any(ri => ri.Ingredient.Name == selected)));

            // Chef filter
            if (!string.IsNullOrWhiteSpace(chef))
                query = query.Where(r => r.Chef != null && r.Chef.Name == chef);

            if (!string.IsNullOrWhiteSpace(dietary))
            {
                if (dietary == "vegetarian")
                {
                    query = query.Where(r => r.IsVegetarian == true);
                }
                else if (dietary == "diaryfree")
                {
                    query = query.Where(r => r.IsDairyFree == true);
                }
            }

            if (!string.IsNullOrEmpty(duration))
            {
                if (duration == "under20")
                {
                    query = query.Where(r => (r.PrepTime + r.CookTime) < 20);
                }
                else if (duration == "20to40")
                {
                    query = query.Where(r => (r.PrepTime + r.CookTime) >= 20 && (r.PrepTime + r.CookTime) <= 40);
                }
                else if (duration == "over40")
                {
                    query = query.Where(r => (r.PrepTime + r.CookTime) > 40);
                }
            }

            recipeCards = await query
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
        }
        // Logic for the reset button to clear the page 
        if (Request.Query.ContainsKey("clear"))
        {
            return View(new SearchPageViewModel
            {
                AllIngredients = allIngredients,
                AllCategories = allCategories,
                AllChefs = allChefs
            });
        }

        // ViewModel yay!
        var viewModel = new SearchPageViewModel
        {
            SearchQuery = searchQuery,
            Recipes = recipeCards,
            AllIngredients = allIngredients,
            AllCategories = allCategories,
            AllChefs = allChefs,
            HasSearch = hasSearch,
            SearchCategory = category,
            SearchChef = chef,
            SelectedIngredients = ingredients,
            Dietary = dietary,
            Difficulty = difficulty,
            DurationBucket = duration
        };

        return View(viewModel);
    }
}