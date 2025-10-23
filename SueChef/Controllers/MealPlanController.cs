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
        var MealPlans = await _db.MealPlans.OrderByDescending(mp => mp.UpdatedOn).ToListAsync();
        return View();
    }


    [Route("/MealPlans")]
    [HttpPost]
    public async Task<IActionResult> Create(MealPlanViewModel mealPlanViewModel)
    {
        int currentUserId = HttpContext.Session.GetInt32("user_id").Value;
        var mealPlan = new MealPlan();

        if (await _db.MealPlans.AnyAsync(mp => mp.MealPlanTitle == mealPlanViewModel.MealPlanTitle))
        {
            ModelState.AddModelError("", "Meal Plan Title must be unique.");
            return View("Index", mealPlanViewModel);
        }
        mealPlan.UserId = currentUserId;
        mealPlan.MealPlanTitle = mealPlanViewModel.MealPlanTitle;

        _db.MealPlans.Add(mealPlan);
        await _db.SaveChangesAsync();
        return View("Index");
    }

}