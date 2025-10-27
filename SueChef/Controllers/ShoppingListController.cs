using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;
using SueChef.ActionFilters;
using Newtonsoft.Json;
using System.Linq;


namespace SueChef.Controllers;

[ServiceFilter(typeof(AuthenticationFilter))]
public class ShoppingListController : Controller
{
    private readonly ILogger<ShoppingListController> _logger;
    private readonly SueChefDbContext _db;

    public ShoppingListController(ILogger<ShoppingListController> logger, SueChefDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    // [HttpPost]
    // [ValidateAntiForgeryToken]
    // public IActionResult Generate(int MealPlanId)
    // {
    //     var recipes = _db.MealPlanRecipes
    //     .Where(mpr => mpr.MealPlanId == MealPlanId && !mpr.IsDeleted)
    //     .Select(mpr => new
    //     {
    //         Ingredients = mpr.Recipe.RecipeIngredients.Select(ri => new
    //         {
    //             Name = ri.Ingredient.Name,
    //             Quantity = ri.Quantity,
    //             Unit = ri.Unit,
    //             Category = ri.Ingredient.Category
    //         })
    //     })
    //     .ToList();
        
        

    //     Dictionary<string, (decimal, string, string)> ShoppingList = new Dictionary<string, (decimal, string, string)>();

    //     foreach (var recipe in recipes)
    //     {
    //         foreach (var ingredient in recipe.Ingredients)
    //         {
    //             if (!ShoppingList.ContainsKey(ingredient.Name))
    //             {
    //                 ShoppingList[ingredient.Name] = (ingredient.Quantity, ingredient.Unit, ingredient.Category);
    //             }
    //             else
    //             {
    //                 decimal newQuant = ShoppingList[ingredient.Name].Item1 + ingredient.Quantity;
    //                 ShoppingList[ingredient.Name] = (newQuant, ingredient.Unit, ingredient.Category);
    //             }    
    //         }
    //     }
    //     TempData["ShoppingList"] = JsonConvert.SerializeObject(ShoppingList);
    //     return RedirectToAction("Show", "MealPlan", new { id = MealPlanId });
    // }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Generate(int MealPlanId, List<int> RecipeIds, List<int> Servings)
    {
        // Pair up recipe IDs and serving multipliers
        var servingsPerRecipe = RecipeIds.Zip(Servings, (id, s) => new { id, s })
                                        .ToDictionary(x => x.id, x => x.s);

        var recipes = _db.MealPlanRecipes
        .Where(mpr => mpr.MealPlanId == MealPlanId && !mpr.IsDeleted)
        .Select(mpr => new
        {
            RecipeId = mpr.RecipeId,
            Ingredients = mpr.Recipe.RecipeIngredients.Select(ri => new
            {
                Name = ri.Ingredient.Name,
                Quantity = ri.Quantity,
                Unit = ri.Unit,
                Category = ri.Ingredient.Category
            })
        })
        .ToList();


        Dictionary<string, Dictionary<string, (decimal, string)>> shoppingList = new Dictionary<string, Dictionary<string, (decimal, string)>>();

        foreach (var recipe in recipes)
        {
            int multiplier = servingsPerRecipe.GetValueOrDefault((int)recipe.RecipeId, 1);
            foreach (var ing in recipe.Ingredients)
            {
                var qty = ing.Quantity * multiplier;
                if (!shoppingList.ContainsKey(ing.Category))
                    shoppingList[ing.Category] = new();

                if (shoppingList[ing.Category].ContainsKey(ing.Name))
                {
                    var existing = shoppingList[ing.Category][ing.Name];
                    shoppingList[ing.Category][ing.Name] = (existing.Item1 + qty, ing.Unit);
                }
                else
                {
                    shoppingList[ing.Category][ing.Name] = (qty, ing.Unit);
                }
            }
        }
        TempData["ShoppingList"] = JsonConvert.SerializeObject(shoppingList);
        return RedirectToAction("Show", "MealPlan", new { id = MealPlanId });
    }
}