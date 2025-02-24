using CyberLoot.Models;
using CyberLoot.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json.Nodes;

namespace BestStoreMVC.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {

        private string PaypalClientId { get; set; } = "";
        private string PaypalSecret { get; set; } = "";
        private string PaypalUrl { get; set; } = "";
        private string SenderName { get; set; } = "";
        private string SenderEmail { get; set; } = "";

        private readonly ApplicationDbContext context;
        private readonly UserManager<ApplicationUser> userManager;

        public CheckoutController(IConfiguration configuration, ApplicationDbContext context
            , UserManager<ApplicationUser> userManager)
        {
            PaypalClientId = configuration["PaypalSettings:ClientId"]!;
            PaypalSecret = configuration["PaypalSettings:Secret"]!;
            PaypalUrl = configuration["PaypalSettings:Url"]!;

            SenderEmail = configuration["BrevoSettings:SenderEmail"]!;
            SenderName = configuration["BrevoSettings:SenderName"]!;


            this.context = context;
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal total = CartHelper.GetSubtotal(cartItems);

            ViewBag.Total = total;
            ViewBag.PaypalClientId = PaypalClientId;
            return View();
        }


        [HttpPost]
        public async Task<JsonResult> CreateOrder()
        {
            List<OrderItem> cartItems = CartHelper.GetCartItems(Request, Response, context);
            decimal totalAmount = CartHelper.GetSubtotal(cartItems);

            // create the request body
            JsonObject createOrderRequest = new JsonObject();
            createOrderRequest.Add("intent", "CAPTURE");

            JsonObject amount = new JsonObject();
            amount.Add("currency_code", "EUR");
            amount.Add("value", totalAmount);

            JsonObject purchaseUnit1 = new JsonObject();
            purchaseUnit1.Add("amount", amount);

            JsonArray purchaseUnits = new JsonArray();
            purchaseUnits.Add(purchaseUnit1);

            createOrderRequest.Add("purchase_units", purchaseUnits);


            // get access token
            string accessToken = await GetPaypalAccessToken();

            // send request
            string url = PaypalUrl + "/v2/checkout/orders";


            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent(createOrderRequest.ToString(), null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                // if the order is successful, return paypal order ID
                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        string paypalOrderId = jsonResponse["id"]?.ToString() ?? "";

                        return new JsonResult(new { Id = paypalOrderId });
                    }
                }
            }

            // otherwise it returns an empty order id
            return new JsonResult(new { Id = "" });
        }

        [HttpPost]
        public async Task<JsonResult> CompleteOrder([FromBody] JsonObject data)
        {
            var orderId = data?["orderID"]?.ToString();

            if (orderId == null)
            {
                return new JsonResult("error");
            }

            // get access token
            string accessToken = await GetPaypalAccessToken();

            // request is sent to paypal API to be confirmed
            string url = PaypalUrl + "/v2/checkout/orders/" + orderId + "/capture";

            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("", null, "application/json");

                var httpResponse = await client.SendAsync(requestMessage);

                // if the order is confirmed it is saved to the database.
                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();
                    var jsonResponse = JsonNode.Parse(strResponse);

                    if (jsonResponse != null)
                    {
                        string paypalOrderStatus = jsonResponse["status"]?.ToString() ?? "";
                        if (paypalOrderStatus == "COMPLETED")
                        {
                            // save the order in the database
                            await SaveOrder(jsonResponse.ToString());

                            return new JsonResult("success");
                        }
                    }
                }
            }

            return new JsonResult("error");
        }

        private async Task SaveOrder(string paypalResponse)
        {
            // get cart items from session
            var cartItems = CartHelper.GetCartItems(Request, Response, context);

            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return;
            }

            // save the order
            var order = new Order
            {
                ClientId = appUser.Id,
                Items = cartItems,
                PaymentMethod = "paypal",
                PaymentStatus = "accepted",
                PaymentDetails = paypalResponse,
                OrderStatus = "completed",
                CreateAt = DateTime.Now,
            };

            context.Order.Add(order);
            context.SaveChanges();

            // Send confirmation email
            await SendOrderConfirmationEmail(appUser, order);

            // delete the shopping cart cookie
            Response.Cookies.Delete("shopping_cart");
        }

        private async Task SendOrderConfirmationEmail(ApplicationUser user, Order order)
        {
            string ordersUrl = Url.ActionLink("Index", "ClientOrders", null, Request.Scheme);
            string subject = "Order Confirmation - Thank You for Your Purchase!";
            var message = $@"
                Dear {user.FirstName} {user.LastName},

                Thank you for your purchase! Your order (ID: {order.Id}) has been successfully completed.

                You can view your order details and track your order status by visiting your orders page:
                {ordersUrl}

                We appreciate your business and hope you enjoy your purchase!

                Best regards,
                CyberLoot Team";

            string senderName = SenderName;
            string senderEmail = SenderEmail;

            EmailSender.SendEmail(senderName, senderEmail, $"{user.FirstName} {user.LastName}", user.Email, subject, message);
        }

        /*
        public async Task<string> Token()
        {
            return await GetPaypalAccessToken();
        }
        */

        private async Task<string> GetPaypalAccessToken()
        {
            string accessToken = "";


            string url = PaypalUrl + "/v1/oauth2/token";

            using (var client = new HttpClient())
            {
                string credentials64 =
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(PaypalClientId + ":" + PaypalSecret));

                client.DefaultRequestHeaders.Add("Authorization", "Basic " + credentials64);

                var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
                requestMessage.Content = new StringContent("grant_type=client_credentials", null
                    , "application/x-www-form-urlencoded");

                var httpResponse = await client.SendAsync(requestMessage);


                if (httpResponse.IsSuccessStatusCode)
                {
                    var strResponse = await httpResponse.Content.ReadAsStringAsync();

                    var jsonResponse = JsonNode.Parse(strResponse);
                    if (jsonResponse != null)
                    {
                        accessToken = jsonResponse["access_token"]?.ToString() ?? "";
                    }
                }
            }


            return accessToken;
        }
    }
}
