using System.Linq;
using _2AuthenticAPP.Data;

namespace _2AuthenticAPP.Models
{
    // Implementation of the IProductRepository interface
    public class EFProductRepository : IProductRepository
    {
        private readonly BorchardtDbContext _context;

        // Constructor injects the DbContext
        public EFProductRepository(BorchardtDbContext ctx)
        {
            _context = ctx;
        }

        // Products property using Entity Framework Core
        public IQueryable<Product> Products => _context.Products;


        //WISHLIST STUFF
            public void AddToWishlist(int customerId, int productId)
            {
                // Implement the logic to add the product to the customer's wishlist
                // You can use the _context to access the database and perform the necessary operations
                // For example, you can create a new record in a "Wishlist" table that associates the customer ID and product ID
            }

            public void RemoveFromWishlist(int customerId, int productId)
            {
                // Implement the logic to remove the product from the customer's wishlist
                // You can use the _context to access the database and perform the necessary operations
                // For example, you can delete the corresponding record from the "Wishlist" table
            }

            public IEnumerable<Product> GetWishlistItems(int customerId)
            {
                // Implement the logic to retrieve the wishlisted products for the given customer
                // You can use the _context to query the database and retrieve the products associated with the customer's wishlist
                // For example, you can join the "Wishlist" table with the "Products" table to get the desired product information
                return Enumerable.Empty<Product>(); // Replace with your actual implementation
            }

        //CART STUFF
            public void AddToCart(int customerId, int productId)
            {
                // Check if the product is already in the customer's cart
                var existingCartItem = _context.CartItems
                    .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

                if (existingCartItem == null)
                {
                    // Create a new cart item
                    var cartItem = new CartItem
                    {
                        CustomerId = customerId,
                        ProductId = productId
                    };

                    // Add the cart item to the database
                    _context.CartItems.Add(cartItem);
                    _context.SaveChanges();
                }
            }
            public void RemoveFromCart(int customerId, int productId)
            {
                // Find the cart item by customer ID and product ID
                var cartItem = _context.CartItems
                    .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

                if (cartItem != null)
                {
                    // Remove the cart item from the database
                    _context.CartItems.Remove(cartItem);
                    _context.SaveChanges();
                }
            }
            public IEnumerable<Product> GetCartItems(int customerId)
            {
                // Retrieve the carted products for the given customer
                var cartedProducts = _context.CartItems
                    .Where(w => w.CustomerId == customerId)
                    .Join(_context.Products,
                        w => w.ProductId,
                        p => p.ProductId,
                        (w, p) => p)
                    .ToList();

                return cartedProducts;
            }

    }
}
