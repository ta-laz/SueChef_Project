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
    public async Task<IActionResult> Index(string? searchQuery, string? category, string? chef, List<string>? ingredients)
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

        var recipeCards = new List<RecipeCardViewModel>();

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

            // Ingredient checkboxes
            if (ingredients != null && ingredients.Any())
                query = query.Where(r => r.RecipeIngredients.Any(ri => ingredients.Contains(ri.Ingredient.Name!)));

            // Chef filter
            if (!string.IsNullOrWhiteSpace(chef))
                query = query.Where(r => r.Chef != null && r.Chef.Name == chef);

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

        var viewModel = new SearchPageViewModel
        {
            SearchQuery = searchQuery,
            Recipes = recipeCards,
            AllIngredients = allIngredients,
            AllCategories = allCategories,
            AllChefs = allChefs
        };

        return View(viewModel);
    }
}