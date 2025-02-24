using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("/Admin/[controller]/{action=Index}/{id?}")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        private readonly int pageSize = 5;

        public ProductsController(ApplicationDbContext context, IWebHostEnvironment environment, IConfiguration configuration, 
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _environment = environment;
            _configuration = configuration;
            _userManager = userManager;
            _userManager = userManager;
        }

        public IActionResult Index(int pageIndex, string? search, string? column, string? orderBy)
        {
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

            // Sort Functionality
            string[] validColumns = { "Id", "Name", "Developer", "Publisher", "Price", "ReleaseDate", "Genre" };
            string[] validOrderBy = { "desc", "asc" };

            if (!validColumns.Contains(column))
            {
                column = "Id";
            }

            if (!validOrderBy.Contains(orderBy))
            {
                orderBy = "desc";
            }

            // Switch statement for sorting
            switch (column)
            {
                case "Name":
                    query = orderBy == "asc" ? query.OrderBy(p => p.Name) : query.OrderByDescending(p => p.Name);
                    break;
                case "Developer":
                    query = orderBy == "asc" ? query.OrderBy(p => p.Developer) : query.OrderByDescending(p => p.Developer);
                    break;
                case "Publisher":
                    query = orderBy == "asc" ? query.OrderBy(p => p.Publisher) : query.OrderByDescending(p => p.Publisher);
                    break;
                case "Price":
                    query = orderBy == "asc" ? query.OrderBy(p => p.Price) : query.OrderByDescending(p => p.Price);
                    break;
                case "ReleaseDate":
                    query = orderBy == "asc" ? query.OrderBy(p => p.ReleaseDate) : query.OrderByDescending(p => p.ReleaseDate);
                    break;
                case "Genre":
                    query = orderBy == "asc" ? query.OrderBy(p => p.ProductGenres.FirstOrDefault().Genre.GenreName) : query.OrderByDescending(p => p.ProductGenres.FirstOrDefault().Genre.GenreName);
                    break;
                default:
                    query = orderBy == "asc" ? query.OrderBy(p => p.Id) : query.OrderByDescending(p => p.Id);
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

            foreach (var product in products)
            {
                product.ProductGenres = product.ProductGenres ?? new List<ProductGenre>();
            }

            ViewData["PageIndex"] = pageIndex;
            ViewData["TotalPages"] = totalPages;

            ViewData["Search"] = search ?? "";

            ViewData["Column"] = column;
            ViewData["OrderBy"] = orderBy;

            return View(products);
        }

        public IActionResult Create()
        {
            ViewData["Genres"] = new SelectList(_context.Genres, "GenreId", "GenreName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductDto productDto)
        { 
            // validate the image file 
            if (productDto.ImageFile == null)
            {
                ModelState.AddModelError("ImageFile", "The image file is required");
            }

            if (!ModelState.IsValid)
            {
                // Reload genres in case of an invalid model state
                ViewBag.Genres = await _context.Genres.ToListAsync();
                return View(productDto);
            }

            var fileName = productDto.ImageFile != null
                ? $"{DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile.FileName)}"
                : null;

            if (fileName != null)
            {
                var filePath = Path.Combine(_environment.WebRootPath, "products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(stream);
                }
            }

            // Create new product from DTO
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Publisher = productDto.Publisher,
                Developer = productDto.Developer,
                Price = productDto.Price,
                GameSize = productDto.GameSize,
                ImageUrl = fileName != null ? "/products/" + fileName : null,
                ReleaseDate = productDto.ReleaseDate,
                ProductGenres = new List<ProductGenre>() // Initialize the list
            };

            // add selected genre to the games.
            foreach (var genreId in productDto.GenreIds)
            {
                product.ProductGenres.Add(new ProductGenre
                {
                    ProductId = product.Id,
                    GenreId = genreId,
                });
            }

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // Get
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _context.Products
                                        .Include(p => p.ProductGenres)
                                        .ThenInclude(pg => pg.Genre)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Create ProductDto from product
            var productDto = new ProductDto
            {
                Name = product.Name,
                Description = product.Description,
                Publisher = product.Publisher,
                Developer = product.Developer,
                Price = product.Price,
                GameSize = product.GameSize,
                ExistingImageUrl = product.ImageUrl,
                ReleaseDate = product.ReleaseDate,
                GenreIds = product.ProductGenres.Select(pg => pg.GenreId).ToList()
            };

            ViewData["ProductId"] = product.Id;
            ViewData["ImageFileName"] = product.ImageUrl;

            ViewBag.Genres = new MultiSelectList(_context.Genres, "GenreId", "GenreName", productDto.GenreIds);

            return View(productDto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ProductDto productDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Genres = new MultiSelectList(_context.Genres, "GenreId", "GenreName", productDto.GenreIds);
                return View(productDto);
            }

            var product = await _context.Products
                                        .Include(p => p.ProductGenres)
                                        .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Publisher = productDto.Publisher;
            product.Developer = productDto.Developer;
            product.Price = productDto.Price;
            product.GameSize = productDto.GameSize;
            product.ReleaseDate = productDto.ReleaseDate;

            // Image File update
            if (productDto.ImageFile != null)
            {
                if (product.ImageUrl != null)
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath); // Old image is deleted
                    }
                }
              
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmssfff") + Path.GetExtension(productDto.ImageFile.FileName);
                var filePath = Path.Combine(_environment.WebRootPath, "products", fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await productDto.ImageFile.CopyToAsync(stream); // new image is saved
                }
                product.ImageUrl = "/products/" + fileName;
            }

            // Clear existing genres from the game
            product.ProductGenres.Clear();

            // Adds selected genres
            foreach (var genreId in productDto.GenreIds)
            {
                product.ProductGenres.Add(new ProductGenre { GenreId = genreId, ProductId = product.Id });
            }

            _context.Products.Update(product);
            await _context.SaveChangesAsync();

            await EmailNotification(product); // sends an email if price hits a user wishlist range.

            return RedirectToAction("Index");
        }

        private async Task EmailNotification(Product product)
        {
            var wishlistItems = await _context.WishlistItems
                                              .Include(w => w.User)
                                              .Where(w => w.ProductId == product.Id)
                                              .ToListAsync();    
            
            foreach (var item in wishlistItems)
            {
                if (product.Price >= item.MinPrice && product.Price <= item.MaxPrice)
                {
                    string wishlistUrl = Url.ActionLink("Index", "Wishlist", null, Request.Scheme);

                    var message = $"The game {product.Name} is now on sale for €{product.Price} within your desired price range!\n" + 
                        $"You can view your wishlist and make a purchase here: {wishlistUrl}";

                    string senderName = _configuration["BrevoSettings:SenderName"] ?? "";
                    string senderEmail = _configuration["BrevoSettings:SenderEmail"] ?? "";
                    string username = item.User.FirstName + " " + item.User.LastName;
                    string subject = "Game within your price range!";

                    Console.WriteLine($"Product {product.Name} updated. Checking for notifications...");
                    EmailSender.SendEmail(senderName, senderEmail, username, item.User.Email, subject, message);
                }
            }
        }

        // Delete
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return RedirectToAction(nameof(Index));
            }

            // Delete the product image
            var imagePath = Path.Combine(_environment.WebRootPath, product.ImageUrl.TrimStart('/'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}