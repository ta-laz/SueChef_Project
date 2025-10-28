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
                ModelState.AddModelError("", "This Meal Plan Title has been used before.");
            }
            else
            {
                _db.MealPlans.Add(new MealPlan
                {
                    UserId = currentUserId,
                    MealPlanTitle = mealPlanViewModel.MealPlanTitle,
                    CreatedOn = DateTime.UtcNow,
                    UpdatedOn = DateTime.UtcNow
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

        var recipes = _db.MealPlanRecipes
            .Where(mpr => mpr.MealPlanId == id && !mpr.IsDeleted)
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

    [Route("/MealPlans/AddRecipe")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRecipe(int recipeId, List<int> mealPlanIds)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");

        if (currentUserId == null)
        {
            TempData["ErrorMessage"] = "You must be logged in to add recipes to a meal plan.";
            return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
        }

        if (mealPlanIds == null || mealPlanIds.Count == 0)
        {
            TempData["ErrorMessage"] = "Please select at least one meal plan.";
            return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
        }

        var mealPlans = await _db.MealPlans
            .Include(mp => mp.MealPlanRecipes)
            .Where(mp => mealPlanIds.Contains(mp.Id) && mp.UserId == currentUserId)
            .ToListAsync();

        if (!mealPlans.Any())
        {
            TempData["ErrorMessage"] = "No valid meal plans found.";
            return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
        }

        int addedCount = 0;
        foreach (var plan in mealPlans)
        {
            // Skip if recipe already exists in plan
            bool exists = plan.MealPlanRecipes.Any(mpr => mpr.RecipeId == recipeId && !mpr.IsDeleted);
            if (exists)
                continue;

            plan.MealPlanRecipes.Add(new MealPlanRecipe
            {
                RecipeId = recipeId,
                MealPlanId = plan.Id,
            });
            addedCount++;
        }

        if (addedCount > 0)
        {
            await _db.SaveChangesAsync();

            if (addedCount == 1)
            {
                string addedTitle = mealPlans.First().MealPlanTitle ?? "Meal Plan";
                TempData["Success"] = $"Recipe added to {addedTitle}.";
            }
            else
            {
                TempData["Success"] = $"Recipe added to {addedCount} meal plans.";
            }
        }
        else
        {
            TempData["ErrorMessage"] = "This recipe is already in all selected meal plans.";
        }

        return RedirectToAction("Index", "RecipeDetails", new { id = recipeId });
    }


    [Route("/MealPlans/DeleteRecipe/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        var recipe = await _db.MealPlanRecipes
                                        .Include(mpr => mpr.Recipe)
                                        .FirstOrDefaultAsync(mpr => mpr.Id == id && !mpr.IsDeleted);
        if (recipe == null)
            return NotFound();
        var mealPlanId = recipe.MealPlanId;
        // Mark as deleted instead of removing
        recipe.IsDeleted = true;
        await _db.SaveChangesAsync();

        // Store info in TempData for success message + undo
        TempData["DeletedRecipeId"] = recipe.Id;
        TempData["DeletedRecipeName"] = recipe.Recipe.Title;
        TempData["MealPlanId"] = recipe.MealPlanId;
        TempData["SuccessMessage"] = $"Recipe: {recipe.Recipe.Title} removed successfully.";

        return RedirectToAction("Show", new { id = mealPlanId });
    }
    


    [Route("/MealPlans/UndoDeleteRecipe/{id}")]
    [HttpGet]
    public async Task<IActionResult> UndoDeleteRecipe(int id)
    {
        var recipe = await _db.MealPlanRecipes
            .Include(mpr => mpr.Recipe)
            .FirstOrDefaultAsync(mpr => mpr.Id == id && mpr.IsDeleted);
        if (recipe == null)
        {
            TempData["ErrorMessage"] = "Unable to undo deletion.";
            return RedirectToAction("Index");
        }
        recipe.IsDeleted = false;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"Recipe: {recipe.Recipe.Title} restored successfully!";
        return RedirectToAction("Show", new { id = recipe.MealPlanId });
    }

    [Route("/MealPlans/DeleteMealPlan/{id}")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteMealPlan(int id)
    {
        var mealPlan = _db.MealPlans.Find(id);
        if (mealPlan == null)
            return NotFound();
        mealPlan.IsDeleted = true; // Mark as deleted instead of removing
        await _db.SaveChangesAsync();

        // Store info in TempData for success message + undo
        TempData["DeletedMealPlanId"] = mealPlan.Id;
        TempData["DeletedMealPlanName"] = mealPlan.MealPlanTitle;
        TempData["SuccessMessage"] = $"{mealPlan.MealPlanTitle} deleted successfully.";

        return RedirectToAction("Index");
    }

    [Route("/MealPlans/UndoDeleteMealPlan/{id}")]
    [HttpGet]
    public async Task<IActionResult> UndoDeleteMealPlan(int id)
    {
        var mealPlan = await _db.MealPlans
            .FirstOrDefaultAsync(mp => mp.Id == id && mp.IsDeleted);
        if (mealPlan == null)
        {
            TempData["ErrorMessage"] = "Unable to undo deletion.";
            return RedirectToAction("Index");
        }
        mealPlan.IsDeleted = false;
        await _db.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{mealPlan.MealPlanTitle} restored successfully";
        return RedirectToAction("Index");
    }


    private async Task<List<MealPlanViewModel>> GetMealPlansForUserAsync(int userId)
    {
        var mealPlans = await _db.MealPlans
            .Where(mp => mp.UserId == userId && !mp.IsDeleted)
            .OrderByDescending(mp => mp.UpdatedOn)
            .Select(mp => new MealPlanViewModel
            {
                Id = mp.Id,
                MealPlanTitle = mp.MealPlanTitle,
                UpdatedOn = mp.UpdatedOn,
                RecipeCount = mp.MealPlanRecipes
                    .Where(mpr => mpr.IsDeleted == false)
                    .Count(),
                RecipePicturePaths = mp.MealPlanRecipes
                    .Where(mpr => mpr.IsDeleted == false)
                    .Select(mpr => mpr.Recipe.RecipePicturePath)
                    .Take(4)
                    .ToList()
            })
            .ToListAsync();

        return mealPlans;
    }

}