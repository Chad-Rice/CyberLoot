using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CyberLoot.Controllers
{
    [Authorize(Roles ="admin")]
    [Route("/Admin/Orders/{action=Index}/{id?}")]
    public class AdminOrdersController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly int pageSize = 5;

        public AdminOrdersController(ApplicationDbContext context)
        {
            this.context = context;
        }
        public IActionResult Index(int pageIndex)
        {
            // Queries the database for the orders, clients and items
            IQueryable<Order> query = context.Order.Include(o => o.Client)
                .Include(o => o.Items).OrderByDescending(o => o.Id);

            if (pageIndex <= 0)
            {
                pageIndex = 1;
            }


            // calculates the total number of pages, allows for buttons so you can jump from page 1 - 5
            decimal count = query.Count();
            int totalPages = (int)Math.Ceiling(count / pageSize);

            // skips the orders from prior pages and shows new items by required pge size e.g. 5
            query = query.Skip((pageIndex - 1) * pageSize).Take(pageSize);

            var orders = query.ToList();

            ViewBag.Orders = orders;
            ViewBag.PageIndex = pageIndex;
            ViewBag.TotalPages = totalPages;

            return View();
        }

        public IActionResult Details(int id)
        {
            // gets the order by it's id
            var order = context.Order.Include(o => o.Client).Include(o => o.Items)
                .ThenInclude(oi => oi.Product).FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                return RedirectToAction("Index");
            }

            // Counts the number of orders placed by the client and stores it in the viewbag.
            ViewBag.NumOrders = context.Order.Where(o => o.ClientId == order.ClientId).Count();

            return View(order);
        }

        // edits the payment/ order status 
        public IActionResult Edit(int id, string? payment_status, string? order_status) 
        {
            var order = context.Order.Find(id);
            if (order == null)
            {
                return RedirectToAction("Index");
            }

            // if there is no status redirect to the details page
            if (payment_status == null && order_status == null)
            {
                return RedirectToAction("Details", new { id });
            }

            if (payment_status != null)
            {
                order.PaymentStatus = payment_status;
            }

            if (order_status != null) 
            {
                order.OrderStatus = order_status;
            }

            context.SaveChanges();

            return RedirectToAction("Details", new { id });
        }
    }
}
