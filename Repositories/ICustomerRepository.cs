using System.Linq;

namespace _2AuthenticAPP.Models
{
    // Interface for customer-specific database operations
    public interface ICustomerRepository
    {
        IQueryable<Customer> Customers { get; }
        // You can add more methods specific to the customer entity here
    }
}
