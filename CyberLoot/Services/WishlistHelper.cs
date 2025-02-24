using CyberLoot.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace CyberLoot.Services
{
    public class WishlistHelper
    {
        public static List<Product> GetWishlistItems(HttpRequest request, ApplicationDbContext context, string userId)
        {
            return context.WishlistItems
                .Include(w => w.Product)
                .Where(w => w.UserId == userId)
                .Select(w => w.Product)
                .ToList();
        }

        public static void AddToWishlist(ApplicationDbContext context, string userId, int productId, decimal minPrice, decimal maxPrice)
        {
            var wishlistItem = new WishlistItem
            {
                UserId = userId,
                ProductId = productId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
            };
            context.WishlistItems.Add(wishlistItem);
            context.SaveChanges();
        }

        public static void RemoveFromWishlist(ApplicationDbContext context, string userId, int productId)
        {
            var wishlistItem = context.WishlistItems
                .FirstOrDefault(w => w.UserId == userId && w.ProductId == productId);
            if (wishlistItem != null)
            {
                context.WishlistItems.Remove(wishlistItem);
                context.SaveChanges();
            }
        }
    }
}
