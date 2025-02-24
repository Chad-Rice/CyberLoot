using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberLoot.Models
{
    public class WishlistItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("ApplicationUser")]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public virtual Product Product { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}
