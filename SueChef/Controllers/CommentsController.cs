using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;

namespace SueChef.Controllers;

public class CommentController : Controller
{
    private readonly ILogger<CommentController> _logger;
    private readonly SueChefDbContext _db;


    public CommentController(ILogger<CommentController> logger, SueChefDbContext db)
    {
        //_logger = logger;
        _db = db;
    }

    [Route("/Recipe/{id}")]
    [HttpPost]
    public async Task<IActionResult> Comment(int userId,  int recipeId, string content)
    {
        int currentUserId = userId;

        if (currentUserId == null) //If user is NOT logged in re-direct to log-in page with error message 
        {
            TempData["ErrorMessage"] = "Please log in to comment on recipes.";
            return RedirectToAction("New", "Sessions");
        }
        if (string.IsNullOrWhiteSpace(content))
        {
            TempData["ErrorMessage"] = "Comment cannot be empty.";
            return RedirectToAction("Index", new { id = recipeId });
        }
        var comment = new Comment 
        {
            UserId = currentUserId,
            CreatedOn = DateTime.UtcNow,
            RecipeId = recipeId,
            Content = content
        };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();
        
        return RedirectToAction("Index", new { id = recipeId }); //Re load the page 
    }
}


