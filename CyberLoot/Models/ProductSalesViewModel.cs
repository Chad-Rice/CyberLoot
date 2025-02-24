namespace CyberLoot.Models
{
    public class ProductSalesViewModel
    {
        public string ProductName { get; set; }
        public int PurchaseCount { get; set; }
        public Product Product { get; set; } // Include this if you want to link to the product details page
    }
}
