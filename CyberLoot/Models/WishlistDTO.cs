namespace CyberLoot.Models
{
    public class WishlistDto
    {
        public int ProductId { get; set; }

        public decimal MinPrice { get; set; }
        public decimal MaxPrice { get; set; }
    }
}
