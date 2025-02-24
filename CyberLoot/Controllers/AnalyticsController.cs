using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CyberLoot.Controllers
{
    [Authorize(Roles = "admin")]  // Restrict access to admin users only
    public class AnalyticsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnalyticsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Get pages visited data
            var pageVisitData = await _context.Analytics
                .GroupBy(a => a.PageVisited)
                .Select(g => new AnalyticsViewModel
                {
                    Page = g.Key,
                    Count = g.Count()
                })
                .ToListAsync();

            ViewBag.PageVisitData = pageVisitData;

            // Get the total sales for each game
            var gameSales = await _context.OrderItems
                                      .GroupBy(oi => oi.Product)
                                      .Select(g => new ProductSalesViewModel
                                      {
                                          ProductName = g.Key.Name,
                                          PurchaseCount = g.Sum(oi => oi.Quantity)
                                      })
                                      .OrderByDescending(ps => ps.PurchaseCount)
                                      .ToListAsync();

            return View(gameSales);
        }
    }
}
