using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;

namespace _2AuthenticAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly BorchardtDbContext _context;

        public HomeController(IProductRepository productRepo, BorchardtDbContext context)
        {
            _productRepo = productRepo;
            _context = context;
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 9)
        {
            var productsQuery = _productRepo.Products
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price,
                    ImgLink = p.ImgLink,
                    AverageRating = _context.LineItems
                        .Where(li => li.ProductId == p.ProductId)
                        .Average(li => (int?)li.Rating)
                });

            // Pagination logic
            var paginatedProducts = await PaginatedList<ProductViewModel>.CreateAsync(productsQuery, pageNumber, pageSize);

            return View(paginatedProducts);
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Where(p => p.ProductId == id)
                .Select(p => new ProductViewModel
                {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Price = p.Price.HasValue ? Convert.ToDecimal(p.Price.Value) : (decimal?)null, // Ensure conversion to decimal
                    ImgLink = p.ImgLink,
                    Description = p.Description,
                    Year = p.Year.HasValue ? (int)p.Year.Value : (int?)null,
                    NumParts = p.NumParts.HasValue ? (int)p.NumParts.Value : (int?)null,
                    PrimaryColor = p.PrimaryColor,
                    SecondaryColor = p.SecondaryColor,
                    Category = p.Category
                }).FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound();
            }

            // Calculate average rating
            product.AverageRating = await _context.LineItems
                .Where(li => li.ProductId == product.ProductId)
                .AverageAsync(li => (double?)li.Rating) ?? 0.0;

            return View(product);
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
