using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;


namespace SueChef.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class MealPlanController : Controller
{
    private readonly ILogger<MealPlanController> _logger;
    private readonly SueChefDbContext _db;

    public MealPlanController(ILogger<MealPlanController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }


    [Route("/MealPlans")]
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var allMealPlans = await GetMealPlansForUserAsync(currentUserId);

        var mealPlansPageViewModel = new MealPlansPageViewModel
        {
            MealPlans = allMealPlans
        };

        return View(mealPlansPageViewModel);
    }


    [Route("/MealPlans")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MealPlanViewModel mealPlanViewModel)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

        if (ModelState.IsValid)
        {
            var exists = await _db.MealPlans
                .AnyAsync(mp => mp.MealPlanTitle == mealPlanViewModel.MealPlanTitle && mp.UserId == currentUserId);

            if (exists)
            {
                ModelState.AddModelError("", "Meal Plan Title must be unique");
            }
            else
            {
                _db.MealPlans.Add(new MealPlan
                {
                    UserId = currentUserId,
                    MealPlanTitle = mealPlanViewModel.MealPlanTitle
                });

                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
        }

        // If validation fails or duplicate name
        var allMealPlans = await GetMealPlansForUserAsync(currentUserId);

        var viewModel = new MealPlansPageViewModel
        {
            MealPlans = allMealPlans,
            MealPlanViewModel = mealPlanViewModel
        };

        return View("Index", viewModel);
    }

    [Route("/MealPlans/{id}")]
    [HttpGet]
    public async Task<IActionResult> Show(int id)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;

        var recipes = _db.MealPlanRecipes.Where(mpr => mpr.MealPlanId == id)
            .Select(mp => new RecipeCardViewModel
            {
                Id = mp.RecipeId,
                Title = mp.Recipe.Title,
                Description = mp.Recipe.Description,
                DifficultyLevel = mp.Recipe.DifficultyLevel,
                IsVegetarian = mp.Recipe.IsVegetarian,
                IsDairyFree = mp.Recipe.IsDairyFree,
                RecipePicturePath = mp.Recipe.RecipePicturePath,
                Category = mp.Recipe.Category
            }).ToList();
        return View(recipes);
    }

    [Route("/MealPlans/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecipe(int id, int recipeId)
    {
        _db.MealPlanRecipes.Add(new MealPlanRecipe
        {
            MealPlanId = id,
            RecipeId = recipeId
        });
        await _db.SaveChangesAsync();
        string MealPlanTitle = await _db.MealPlanRecipes.Where(mp => mp.MealPlanId == id).Select(mp => mp.MealPlan.MealPlanTitle).FirstOrDefaultAsync();
        TempData["Success"] = $"Recipe added to meal plan {MealPlanTitle}";
        return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
    }

    private async Task<List<MealPlanViewModel>> GetMealPlansForUserAsync(int userId)
    {
        var mealPlans = await _db.MealPlans
            .Where(mp => mp.UserId == userId)
            .OrderByDescending(mp => mp.UpdatedOn)
            .Select(mp => new MealPlanViewModel
            {
                Id = mp.Id,
                MealPlanTitle = mp.MealPlanTitle,
                UpdatedOn = mp.UpdatedOn,
                RecipeCount = mp.MealPlanRecipes.Count()
            })
            .ToListAsync();

        return mealPlans;
    }

}