using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SueChef.Models;
using SueChef.ViewModels;


namespace SueChef.Controllers;

    public class RatingController : Controller
    {
        private readonly ILogger<SessionsController> _logger;
        private readonly SueChefDbContext _db;

        public RatingController(ILogger<RatingController> logger, SueChefDbContext db)
        {
            //_logger = logger;
            _db = db;
        }
        [Route("/Recipe/{id}")]
        [HttpGet]
        public async Task<IActionResult> Index(int id)
        {
            
        }
    }
