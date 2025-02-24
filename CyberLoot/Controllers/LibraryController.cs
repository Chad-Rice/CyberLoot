using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Text;

namespace CyberLoot.Controllers
{
    [Authorize(Roles = "client")]
    [Route("/Client/Library/{action=Index}")]
    public class LibraryController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LibraryController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Uses Orders to find users bought games
            var ownedGames = await _context.Order
                                           .Where(o => o.ClientId == user.Id && o.OrderStatus == "completed")
                                           .Include(o => o.Items)
                                           .ThenInclude(oi => oi.Product)
                                           .SelectMany(o => o.Items.Select(oi => oi.Product))
                                           .ToListAsync();

            return View(ownedGames);
        }

        public async Task<IActionResult> AddToDownload(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Retrieves current download queue
            var downloadQueueJson = HttpContext.Session.GetString("DownloadQueue");
            var downloadQueue = string.IsNullOrEmpty(downloadQueueJson)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(downloadQueueJson);

            // Add the selected game to downloads
            if (!downloadQueue.Any(p => p.Id == product.Id))
            {
                downloadQueue.Add(product);
                HttpContext.Session.SetString("DownloadQueue", JsonConvert.SerializeObject(downloadQueue));
            }

            return RedirectToAction("Index");
        }

        // Displays the download queue
        public IActionResult Downloads()
        {
            // Retrieve the queue from the session
            var downloadQueueJson = HttpContext.Session.GetString("DownloadQueue");
            var downloadQueue = string.IsNullOrEmpty(downloadQueueJson)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(downloadQueueJson);

            // Pass the current download and the queue to the view model
            var viewModel = new DownloadViewModel
            {
                CurrentDownload = downloadQueue.FirstOrDefault(),
                Queue = downloadQueue.Skip(1).ToList()
            };

            return View(viewModel);
        }

        public async Task<IActionResult> Download(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Retrieves current download queue
            var downloadQueueJson = HttpContext.Session.GetString("DownloadQueue");
            var downloadQueue = string.IsNullOrEmpty(downloadQueueJson)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(downloadQueueJson);

            // Remove the product from the queue if it is already in the queue (adds it to the front of the queue)
            downloadQueue.RemoveAll(p => p.Id == id);

            // Add the currently downloading game back to the queue if it's not completed
            if (downloadQueue.Any() && downloadQueue.First().Id != id)
            {
                downloadQueue.Insert(1, downloadQueue.First());
                downloadQueue.RemoveAt(0);
            }
            // Add the new game to the front of the queue
            downloadQueue.Insert(0, product);

            // Update the session with the new queue
            HttpContext.Session.SetString("DownloadQueue", JsonConvert.SerializeObject(downloadQueue));

            // Pass the current download and the queue to the view model
            var viewModel = new DownloadViewModel
            {
                CurrentDownload = product,
                Queue = downloadQueue.Skip(1).ToList() // Skip the first one since it's the current download
            };

            return View("Downloads", viewModel);
        }

        public async Task<IActionResult> CompleteDownload(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            // Simulate file generation (dummy file content)
            var fileName = product.Name + ".txt";
            var fileContent = $"This is a simulated download for the game: {product.Name}.\n In a real download, obviously you would be downloading multiple folders from the database.";

            var byteArray = Encoding.UTF8.GetBytes(fileContent);
            var stream = new MemoryStream(byteArray);

            // Remove game from queue when download is done
            var downloadQueueJson = HttpContext.Session.GetString("DownloadQueue");
            var downloadQueue = string.IsNullOrEmpty(downloadQueueJson)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(downloadQueueJson);

            var gameToRemove = downloadQueue.FirstOrDefault(product => product.Id == id);
            if (gameToRemove != null)
            {
                downloadQueue.Remove(gameToRemove);
                HttpContext.Session.SetString("DownloadQueue", JsonConvert.SerializeObject(downloadQueue));
            }

            // Return the dummy file as a download
            return RedirectToAction("Downloads");
        }

        public IActionResult RemoveFromDownloadQueue(int id)
        {
            // Retrieve the current download queue from the session
            var downloadQueueJson = HttpContext.Session.GetString("DownloadQueue");
            var downloadQueue = string.IsNullOrEmpty(downloadQueueJson)
                ? new List<Product>()
                : JsonConvert.DeserializeObject<List<Product>>(downloadQueueJson);

            // Find the product to remove
            var gameToRemove = downloadQueue.FirstOrDefault(product => product.Id == id);
            if (gameToRemove != null)
            {
                downloadQueue.Remove(gameToRemove);
                HttpContext.Session.SetString("DownloadQueue", JsonConvert.SerializeObject(downloadQueue));
            }

            return RedirectToAction("Downloads");
        }
    }
}
