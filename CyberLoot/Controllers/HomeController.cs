using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CyberLoot.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public HomeController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // tracks home page visits that aren't by an admin
            if (user != null && !await _userManager.IsInRoleAsync(user, "admin"))
            {
                _context.Analytics.Add(new Analytics
                {
                    UserId = user.Id,
                    VisitDate = DateTime.UtcNow,
                    PageVisited = "Home"
                });
                await _context.SaveChangesAsync();
            }

            // shows the latest games added
            var products = _context.Products
                           .Include(p => p.ProductGenres)
                           .ThenInclude(pg => pg.Genre)
                           .OrderByDescending(p => p.ReleaseDate)
                           .Take(4)
                           .ToList(); 
            return View(products);
        }

        // handles errors and displays error page
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
