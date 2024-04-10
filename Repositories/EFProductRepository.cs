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
    }
}
