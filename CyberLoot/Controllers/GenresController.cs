using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberLoot.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class GenresController : Controller
    {
        private readonly ApplicationDbContext _context;

        public GenresController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // gets all genres from the database asynchrounously
            var genres = await _context.Genres.ToListAsync();
            return View(genres);
        }

        // GET: Admin/Genres/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Genres/Create
        [HttpPost]
        [ValidateAntiForgeryToken] // stops request from third party sites
        public async Task<IActionResult> Create(Genre genre)
        {
            if (ModelState.IsValid)
            {
                _context.Genres.Add(genre);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(genre);
        }

        // POST: Admin/Genres/Delete/5
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var genre = await _context.Genres.FindAsync(id);
            if (genre != null)
            {
                _context.Genres.Remove(genre);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
