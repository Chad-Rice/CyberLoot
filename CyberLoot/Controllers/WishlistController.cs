using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace CyberLoot.Controllers
{
    [Authorize]
    public class WishlistController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WishlistController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] WishlistDto wishlistDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // CHeck if game is already in the wishlist
            var existingItem = await _context.WishlistItems
                                             .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == wishlistDto.ProductId);

            if (existingItem != null)
            {
                // Update the price range if the product is already in the wishlist
                existingItem.MinPrice = wishlistDto.MinPrice;
                existingItem.MaxPrice = wishlistDto.MaxPrice;
            }
            else
            {
                // Add a new wishlist item
                var wishlistItem = new WishlistItem
                {
                    UserId = user.Id,
                    ProductId = wishlistDto.ProductId,
                    MinPrice = wishlistDto.MinPrice,
                    MaxPrice = wishlistDto.MaxPrice
                };

                _context.WishlistItems.Add(wishlistItem);
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePriceRange([FromBody] List<WishlistDto> wishlistDtos)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            // iterate through every wishlist item and update it's price range.
            foreach (var dto in wishlistDtos)
            {
                var wishlistItem = await _context.WishlistItems
                                                 .FirstOrDefaultAsync(w => w.UserId == user.Id && w.ProductId == dto.ProductId);

                if (wishlistItem != null)
                {
                    wishlistItem.MinPrice = dto.MinPrice;
                    wishlistItem .MaxPrice = dto.MaxPrice;
                }
                Console.WriteLine($"Updating Wishlist: ProductId: {dto.ProductId}, MinPrice: {dto.MinPrice}, MaxPrice: {dto.MaxPrice}");
            }

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> Remove([FromBody] WishlistDto wishlistDto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            WishlistHelper.RemoveFromWishlist(_context, user.Id, wishlistDto.ProductId);
            return Json(new { success = true });
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }

            if (user != null && !await _userManager.IsInRoleAsync(user, "admin"))
            {
                _context.Analytics.Add(new Analytics
                {
                    UserId = user.Id,
                    VisitDate = DateTime.UtcNow,
                    PageVisited = "Wishlist"
                });
                await _context.SaveChangesAsync();
            }

            var wishlistItems = await _context.WishlistItems
                                      .Include(w => w.Product)
                                      .Where(w => w.UserId == user.Id)
                                      .ToListAsync();

            return View(wishlistItems);
        }
    }
}
