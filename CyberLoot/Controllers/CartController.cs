using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CyberLoot.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CartController(ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        public IActionResult Index()
        {
            // gets the cart items and subtotal
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);


            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal;

            return View();
        }

        [Authorize]
        [HttpPost]
        public IActionResult Index(CheckoutDto model)
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal subtotal = CartHelper.GetSubtotal(cartItems);

            ViewBag.CartItems = cartItems;
            ViewBag.Subtotal = subtotal;
            ViewBag.Total = subtotal;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Check if shopping cart is empty
            if (cartItems.Count == 0)
            {
                ViewBag.ErrorMessage = "Your cart is empty";
                return View(model);
            }

            // Stores selected payment method, in this case only paypal could add more later e.g. google pay
            TempData["PaymentMethod"] = model.PaymentMethod;

            if (model.PaymentMethod == "paypal" || model.PaymentMethod == "credit_card")
            {
                return RedirectToAction("Index", "Checkout");
            }

            return RedirectToAction("Confirm");
        }

        public IActionResult Confirm()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = CartHelper.GetSubtotal(cartItems);
            int cartSize = 0;

            // calculate the number of items in the cart
            foreach (var items in cartItems)
            {
                cartSize += items.Quantity;
            }

            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            // Redirects to the home page if the cart or payment method are empty
            if (cartSize == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.PaymentMethod = paymentMethod;
            ViewBag.Total = total;
            ViewBag.CartSize = cartSize;

            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Confirm(int unneeded)
        {
            var cartItems = CartHelper.GetCartItems(Request, Response, context);
            string paymentMethod = TempData["PaymentMethod"] as string ?? "";
            TempData.Keep();

            if (cartItems.Count == 0 || paymentMethod.Length == 0)
            {
                return RedirectToAction("Index", "Home");
            }

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // save the order
            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                PaymentMethod = paymentMethod,
                PaymentStatus = "accepted",
                PaymentDetails = "",
                OrderStatus = "completed",
                CreateAt = DateTime.Now,
            };

            context.Order.Add(order);

            // Increment Purchase count of each game in the cart (used for analytics)
            foreach(var item in cartItems)
            {
                var product = await context.Products.FindAsync(item.Product.Id);
                if (product != null)
                {
                    product.PurchaseCount += item.Quantity;
                }
            }
            
            await context.SaveChangesAsync();

            // delete shopping cart cookie
            Response.Cookies.Delete("shopping_cart");

            ViewBag.SuccessMessage = "Purchase was successful, please return to the home page.";

            return View();
        }

    }
}