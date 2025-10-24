using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Controllers;

public class CommentController : Controller
{
    private readonly SueChefDbContext _db;


    public CommentController(ILogger<CommentController> logger, SueChefDbContext db)
    {
        _db = db;
    }

    [Route("/recipe/{recipeId}/comments", Name = "commentingOnRecipe")]
    [HttpPost]
    public async Task<IActionResult> comments(int recipeId, string content)
    {
        int? currentUserId = HttpContext.Session.GetInt32("user_id");
        if (!currentUserId.HasValue) return RedirectToAction("New", "Sessions");

        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Comment cannot be empty.";
            return Redirect($"/recipe/{recipeId}");
        }

        _db.Comments.Add(new Comment {
            UserId = currentUserId.Value,
            RecipeId = recipeId,
            Content = content.Trim(),
            CreatedOn = DateTime.UtcNow
        });
        await _db.SaveChangesAsync();

        return Redirect($"/recipe/{recipeId}"); // back to details
    }
}


