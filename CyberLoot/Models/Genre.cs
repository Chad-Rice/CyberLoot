using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CyberLoot.Models
{
    [Table("Genres")]
    public class Genre
    {
        public int GenreId { get; set; }

        [Required]
        [MaxLength(100)]
        public string GenreName { get; set; } = "";

        // Navigation
        public ICollection<ProductGenre> ProductGenres { get; set; } = new List<ProductGenre>(); // Navigation
    }
}
