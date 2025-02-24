using CyberLoot.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CyberLoot.Services
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options) 
        {

        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Genre> Genres { get; set; }
        public DbSet<ProductGenre> ProductGenres { get; set; }
        public DbSet<Order> Order { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        public DbSet<WishlistItem> WishlistItems { get; set; }
        public DbSet<Analytics> Analytics { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ProductGenre>()
                .HasKey(pg => new {pg.ProductId, pg.GenreId});

            modelBuilder.Entity<ProductGenre>()
             .HasOne(pg => pg.Product)
             .WithMany(p => p.ProductGenres)
             .HasForeignKey(pg => pg.ProductId);

            modelBuilder.Entity<ProductGenre>()
                .HasOne(pg => pg.Genre)
                .WithMany(g => g.ProductGenres)
                .HasForeignKey(pg => pg.GenreId);

            // Set precision for wishlist decimal
            modelBuilder.Entity<WishlistItem>()
                        .Property(w => w.MinPrice)
                        .HasPrecision(18, 2);

            modelBuilder.Entity<WishlistItem>()
                        .Property(w => w.MaxPrice)
                        .HasPrecision(18, 2);

            base.OnModelCreating(modelBuilder);

        }
    }
}
