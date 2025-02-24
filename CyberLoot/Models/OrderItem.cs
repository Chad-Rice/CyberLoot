using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberLoot.Models
{
    [Table("OrderItems")]
    public class OrderItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }

        [Precision(16, 2)]
        public decimal UnitPrice { get; set; }

        // Foreign Key
        public int OrderId { get; set; }

        // navigation property
        public Product Product { get; set; } = new Product();

        // navigation property for Order
        public Order Order { get; set; }
    }
}
