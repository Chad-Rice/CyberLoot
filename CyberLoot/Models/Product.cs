using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Models
{
    public class Product
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Publisher { get; set; } = "";

        [MaxLength(100)]
        public string Developer { get; set; } = "";

        [Precision(16, 2)]
        public decimal Price { get; set; }
        [MaxLength(100)]
        public string ImageUrl { get; set; } = "";
        public DateTime ReleaseDate { get; set; }
        public int PurchaseCount { get; set; } = 0;
        public int GameSize { get; set; } = 0; // Size in MB
        public ICollection<ProductGenre> ProductGenres { get; set; }// Navigation
    }
}
