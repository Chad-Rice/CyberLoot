using System.ComponentModel.DataAnnotations;

namespace CyberLoot.Models
{
    public class ProductDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        [Required]
        public string Description { get; set; } = "";

        [Required]
        public string Publisher { get; set; }

        [Required, MaxLength(100)]
        public string Developer { get; set; } = "";

        [Required, Range(0.1, 1000.00)]
        public decimal Price { get; set; }
        public IFormFile? ImageFile { get; set; }

        // For Image update
        public string? ExistingImageUrl { get; set; }

        [Required]
        public DateTime ReleaseDate { get; set; }
        public int GameSize { get; set; } = 0; // Size in MB

        public List<int> GenreIds { get; set; } = new List<int>();
    }
}
