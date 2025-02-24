using Microsoft.EntityFrameworkCore;

namespace CyberLoot.Models
{
    public class Order
    {
        public int Id { get; set; }

        public string ClientId { get; set; } = "";
        public ApplicationUser Client { get; set; } = null!;

        public List<OrderItem> Items { get; set; } = new List<OrderItem>();

        public string PaymentMethod { get; set; } = "";
        public string PaymentStatus { get; set; } = "";
        public string PaymentDetails { get; set; } = ""; // For Paypal details
        public string OrderStatus { get; set; } = "";
        public DateTime CreateAt { get; set; }

        // Navigation for Library
        public ICollection<Product> OwnedGames => Items.Select(i => i.Product).ToList();
    }
}
