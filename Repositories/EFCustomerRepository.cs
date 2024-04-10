using System.Linq;
using _2AuthenticAPP.Data;

namespace _2AuthenticAPP.Models
{
    // Implementation of the ICustomerRepository interface
    public class EFCustomerRepository : ICustomerRepository
    {
        private readonly BorchardtDbContext _context;

        // Constructor injects the DbContext
        public EFCustomerRepository(BorchardtDbContext ctx)
        {
            _context = ctx;
        }

        // Customers property using Entity Framework Core
        public IQueryable<Customer> Customers => _context.Customers;
    }
}
