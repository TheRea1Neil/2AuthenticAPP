using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace _2AuthenticAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly BorchardtDbContext _context;

        public HomeController(BorchardtDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Retrieve all customers from the database
            var customers = await _context.Customers.ToListAsync();

            // Store the list of customers in a ViewBag to pass to the view
            ViewBag.Customers = customers;

            return View();
        }

        public IActionResult Details(int id)
        {
            // TODO: Retrieve the product details based on the provided id from the database
            // For now, we'll return the Details view with a hardcoded product ID
            return View("Details", id);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [Authorize]
        public IActionResult Secrets()
        {
            return View();
        }
    }
}
