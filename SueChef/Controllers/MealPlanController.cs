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
            .Select(mpr => new RecipeCardViewModel
            {
                Id = mpr.RecipeId,
                Title = mpr.Recipe.Title,
                Description = mpr.Recipe.Description,
                DifficultyLevel = mpr.Recipe.DifficultyLevel,
                IsVegetarian = mpr.Recipe.IsVegetarian,
                IsDairyFree = mpr.Recipe.IsDairyFree,
                RecipePicturePath = mpr.Recipe.RecipePicturePath,
                Category = mpr.Recipe.Category,
                MealPlanRecipeId = mpr.Id,
                Ingredients = mpr.Recipe.RecipeIngredients.Select(ri => new IndividualRecipeIngredientViewModel
                {
                    Name = ri.Ingredient.Name,
                    Calories = ri.Ingredient.Calories,
                    Carbs = ri.Ingredient.Carbs,
                    Protein = ri.Ingredient.Protein,
                    Fats = ri.Ingredient.Fat,
                    Quantity = ri.Quantity,
                    Unit = ri.Unit
                }).ToList(),
            }).ToList();
        var mealPlan = _db.MealPlans
        .Where(mp => mp.Id == id)
        .Select(mp => new MealPlanViewModel
        {
            Id = mp.Id,
            MealPlanTitle = mp.MealPlanTitle
        })
        .FirstOrDefault();

    var viewModel = new SingleMealPlanPageViewModel
    {
        RecipesList = recipes,
        MealPlan = mealPlan
    };

    return View(viewModel);
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

    [Route("/MealPlans/DeleteRecipe/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var recipe = _db.MealPlanRecipes.Find(id);
        if (recipe == null)
            return NotFound();
        var mealPlanId = recipe.MealPlanId;
        _db.MealPlanRecipes.Remove(recipe);
        await _db.SaveChangesAsync();
        return RedirectToAction("Show", new {id = mealPlanId});
    }

    [Route("/MealPlans/DeleteMealPlan/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMealPlan(int id)
    {
        var mealPlan = _db.MealPlans.Find(id);
        if (mealPlan == null)
            return NotFound();
        _db.MealPlans.Remove(mealPlan);
        await _db.SaveChangesAsync();
        return RedirectToAction("Index");
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
                RecipeCount = mp.MealPlanRecipes.Count(),
                RecipePicturePaths = mp.MealPlanRecipes
                    .Select(mpr => mpr.Recipe.RecipePicturePath)
                    .Take(4)
                    .ToList()
            })
            .ToListAsync();

        return mealPlans;
    }

}