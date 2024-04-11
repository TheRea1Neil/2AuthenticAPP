using System.Linq;
using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models.ViewModels;

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
                var existingCartItem = _context.CartItems
                    .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

                if (existingCartItem != null)
                {
                    existingCartItem.Qty++; // Increment the quantity if the item already exists
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CustomerId = customerId,
                        ProductId = productId,
                        Qty = 1
                    };

                    _context.CartItems.Add(cartItem);
                }

                _context.SaveChanges();
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

            public IEnumerable<CartItemViewModel> GetCartItems(int customerId)
            {
                var cartedProducts = _context.CartItems
                    .Where(w => w.CustomerId == customerId)
                    .Join(_context.Products,
                        w => w.ProductId,
                        p => p.ProductId,
                        (w, p) => new CartItemViewModel
                        {
                            ProductId = p.ProductId,
                            Name = p.Name,
                            Price = p.Price,
                            ImgLink = p.ImgLink,
                            Description = p.Description,
                            Year = p.Year,
                            NumParts = p.NumParts,
                            PrimaryColor = p.PrimaryColor,
                            SecondaryColor = p.SecondaryColor,
                            Category = p.Category,
                            Qty = w.Qty
                        })
                    .ToList();

                return cartedProducts;
            }

            public void IncreaseQuantity(int customerId, int productId)
            {
                var cartItem = _context.CartItems
                    .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

                if (cartItem != null)
                {
                    cartItem.Qty++;
                    _context.SaveChanges();
                }
            }

            public void DecreaseQuantity(int customerId, int productId)
            {
                var cartItem = _context.CartItems
                    .FirstOrDefault(w => w.CustomerId == customerId && w.ProductId == productId);

                if (cartItem != null && cartItem.Qty > 1)
                {
                    cartItem.Qty--;
                    _context.SaveChanges();
                }
            }

    }
}
