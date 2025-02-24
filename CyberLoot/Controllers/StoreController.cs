using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberLoot.Controllers
{
    public class StoreController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly int pageSize = 8;

        public StoreController(ApplicationDbContext context, UserManager<ApplicationUser> userManager) 
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int pageIndex, string? search, string? sort)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null && !await _userManager.IsInRoleAsync(user, "admin"))
            {
                _context.Analytics.Add(new Analytics
                {
                    UserId = user.Id,
                    VisitDate = DateTime.UtcNow,
                    PageVisited = "Store"
                });
                await _context.SaveChangesAsync();
            }

            IQueryable<Product> query = _context.Products
                                                .Include(p => p.ProductGenres)
                                                .ThenInclude(pg => pg.Genre);

            // Search Functionality
            if (!string.IsNullOrWhiteSpace(search)) // string has no whitespace and isn't null
            {
                query = query.Where(p => p.Name.Contains(search) ||
                                         p.Developer.Contains(search) ||
                                         p.Publisher.Contains(search) ||
                                         p.ProductGenres.Any(pg => pg.Genre.GenreName.Contains(search)));
            }

            // sort functionality
            switch(sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "popular":
                    query = query.OrderByDescending(p => _context.OrderItems
                                                                 .Where(oi => oi.Product.Id == p.Id)
                                                                 .Sum(oi => oi.Quantity));
                    break;
                default:
                    query = query.OrderByDescending(p => p.ReleaseDate);
                    break;
            }

            // Pagination Functionailty
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }

            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var products = query.ToList();

            // Check current user's owned games
            if (user != null)
            {
                var ownedGames = await _context.OrderItems
                                           .Where(oi => oi.Order.ClientId == user.Id && oi.Order.OrderStatus == "completed")
                                           .Select(oi => oi.Product.Id)
                                           .ToListAsync();

                ViewBag.OwnedGames = ownedGames;
            }

            // makes sure game's genres are not null. 
            foreach (var product in products)
            {
                product.ProductGenres = product.ProductGenres ?? new List<ProductGenre>();
            }

            ViewBag.Products = products;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            var storeSearchModel = new StoreSearchModel() 
            {
                Search = search,
                Sort = sort,
            };

            return View(storeSearchModel);
        }

        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            // find the product by it's id and include it's genres
            var product = _context.Products
                                  .Include(p => p.ProductGenres)
                                  .ThenInclude(pg => pg.Genre)
                                  .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return RedirectToAction("Index", "Store");
            }

            bool userOwnsProduct = false;

            if (user != null)
            {
                userOwnsProduct = await _context.OrderItems
                                                .AnyAsync(oi => oi.Product.Id == id &&
                                                          oi.Order.ClientId == user.Id &&
                                                          oi.Order.OrderStatus == "completed");
            }

            ViewBag.UserOwnsProduct = userOwnsProduct;

            return View(product);
        }
    }
}
