using _2AuthenticAPP.Models.ViewModels;
using System.Linq;

namespace _2AuthenticAPP.Models
{
    // Interface for product-specific database operations
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }

        //wishlist
        void AddToWishlist(int customerId, int productId);
        void RemoveFromWishlist(int customerId, int productId);
        IEnumerable<Product> GetWishlistItems(int customerId);

        //cart
        void AddToCart(int customerId, int productId);
        void RemoveFromCart(int customerId, int productId);
        void IncreaseQuantity(int customerId, int productId);
        void DecreaseQuantity(int customerId, int productId);
        IEnumerable<CartItemViewModel> GetCartItems(int customerId);
    }
}
