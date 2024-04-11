using System.Linq;

namespace _2AuthenticAPP.Models
{
    // Interface for product-specific database operations
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }

        //wishlist
        void AddToWishlist(long customerId, int productId);
        void RemoveFromWishlist(long customerId, int productId);
        IEnumerable<Product> GetWishlistItems(long customerId);

        //cart
        void AddToCart(long customerId, int productId);
        void RemoveFromCart(long customerId, int productId);
        IEnumerable<Product> GetCartItems(long customerId);
    }
}
