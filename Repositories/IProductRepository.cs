using System.Linq;

namespace _2AuthenticAPP.Models
{
    // Interface for product-specific database operations
    public interface IProductRepository
    {
        IQueryable<Product> Products { get; }
        // You can add more methods specific to the product entity here
    }
}
