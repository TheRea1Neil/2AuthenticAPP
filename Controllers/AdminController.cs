using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models;
using _2AuthenticAPP.Models.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace _2AuthenticAPP.Controllers
{
    public class AdminController : Controller
    {
        private readonly BorchardtDbContext _context;
        private readonly UserRolesService _userRolesService;
        private readonly UserManager<IdentityUser> _userManager; // Add this line
        private readonly RoleManager<IdentityRole> _roleManager;

        // Modify the constructor to include UserManager<IdentityUser>
        public AdminController(BorchardtDbContext context, UserRolesService userRolesService, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userRolesService = userRolesService;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> MakeUserAdmin(string userId)
        {
            var result = await _userRolesService.AddUserToAdminRoleAsync(userId);
            if (result)
            {
                // Successfully added user to the Admin role
                
            }
            else
            {
                // Failed to add user to the Admin role
            }
            return View(result);
            // Redirect or return a view as necessary
        }




        public async Task<IActionResult> Customers(string searchString, int? pageNumber)
        {
            int pageSize = 50;
            var customers = from c in _context.Customers
                            orderby c.Email != null descending, c.CustomerId
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = (IOrderedQueryable<Customer>)customers.Where(c => c.FirstName.Contains(searchString) || c.LastName.Contains(searchString) || c.Email.Contains(searchString));
            }

            return View(await PaginatedList<Customer>.CreateAsync(customers.AsNoTracking(), pageNumber ?? 1, pageSize));
        }

        //Edit/Delete
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound();
            }
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FirstName,LastName,BirthDate,CountryOfResidence,Gender,Age,Email")] Customer customer)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(customer);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CustomerExists(customer.CustomerId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Customers));
            }
            return View(customer);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(m => m.CustomerId == id);

            if (customer == null)
            {
                return NotFound();
            }

            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Disable triggers on AspNetUsers table
            await _context.Database.ExecuteSqlRawAsync("DISABLE TRIGGER ALL ON AspNetUsers");

            try
            {
                var customer = await _context.Customers.FindAsync(id);
                if (customer != null)
                {
                    var user = await _userManager.FindByEmailAsync(customer.Email);
                    if (user != null)
                    {
                        // Perform user deletion
                        var result = await _userManager.DeleteAsync(user);
                        if (result.Succeeded)
                        {
                            // If user deletion succeeded, remove the customer
                            _context.Customers.Remove(customer);
                            await _context.SaveChangesAsync();
                        }
                        else
                        {
                            // Consider how to handle the failure in your application context
                        }
                    }
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                // Re-enable triggers on AspNetUsers table regardless of the outcome
                await _context.Database.ExecuteSqlRawAsync("ENABLE TRIGGER ALL ON AspNetUsers");
            }

            // Redirect to the customer list page or appropriate view
            return RedirectToAction(nameof(Customers));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }

        public async Task<IActionResult> Orders(string searchString, bool showFraudOnly = false, int? pageNumber = 1)
        {
            int pageSize = 10;

            var orders = _context.Orders
                .Include(o => o.Customer) // Include the Customer navigation property
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                orders = orders.Where(o => o.TransactionId.ToString().Contains(searchString) ||
                                           o.Customer.Email.Contains(searchString));
            }

            if (showFraudOnly)
            {
                orders = orders.Where(o => o.Fraud == 1);
            }

            var totalOrders = await orders.CountAsync();
            var paginatedOrders = await orders.Skip((pageNumber.Value - 1) * pageSize).Take(pageSize).ToListAsync();

            var viewModel = new OrderViewModel
            {
                Orders = paginatedOrders,
                SearchString = searchString,
                ShowFraudOnly = showFraudOnly,
                PageNumber = pageNumber.Value,
                TotalPages = (int)Math.Ceiling(totalOrders / (double)pageSize)
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole(int customerId, string role)
        {
            var customer = await _context.Customers.FindAsync(customerId);
            if (customer != null)
            {
                var user = await _userManager.FindByEmailAsync(customer.Email);
                if (user != null)
                {
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            return RedirectToAction(nameof(Customers));
        }
        // Add more admin-specific actions as needed
    }
}