using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models;

namespace _2AuthenticAPP.Controllers
{
    //[Authorize(Roles = "Admin")] not yet
    public class AdminController : Controller
    {
        private readonly BorchardtDbContext _context;
        private readonly UserRolesService _userRolesService;

        public AdminController(BorchardtDbContext context, UserRolesService userRolesService)
        {
            _context = context;
            _userRolesService = userRolesService;
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
                            select c;

            if (!string.IsNullOrEmpty(searchString))
            {
                customers = customers.Where(c => c.FirstName.Contains(searchString) || c.LastName.Contains(searchString) || c.Email.Contains(searchString));
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

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Customers));
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.CustomerId == id);
        }
        // Add more admin-specific actions as needed
    }
}