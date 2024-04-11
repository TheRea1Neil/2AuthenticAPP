using _2AuthenticAPP.Data;
using _2AuthenticAPP.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.AspNetCore.Identity;
using _2AuthenticAPP.Models.ViewModels;
using Microsoft.AspNetCore.Authentication;

namespace _2AuthenticAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly BorchardtDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public HomeController(IProductRepository productRepo, BorchardtDbContext context, UserManager<IdentityUser> userManager)
        {
            _productRepo = productRepo;
            _context = context;
            _userManager = userManager;
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

            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Get the IdentityUser object for the logged-in user
                IdentityUser user = await _userManager.GetUserAsync(User);

                // Query the database to find the customer with the matching email
                var customer = await _context.Customers
                                             .FirstOrDefaultAsync(c => c.Email == user.Email);
            }

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

        //Wishlist Items
        public IActionResult Wishlist(int customerId)
        {
            var wishlistedItems = _productRepo.GetWishlistItems(customerId);
            var viewModel = wishlistedItems.Select(p => new ProductViewModel
            {
                // Map the product properties to the view model
            });
            return View(viewModel);
        }
        [Authorize]
        [HttpPost]
        public IActionResult AddToWishlist(int customerId, int productId)
        {

            _productRepo.AddToWishlist(customerId, productId);
            return Ok();
        }

        [HttpPost]
        public IActionResult RemoveFromWishlist(int customerId, int productId)
        {
            _productRepo.RemoveFromWishlist(customerId, productId);
            return Ok();
        }


    //Cart Items
        public async Task<IActionResult> Cart()
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get the IdentityUser object for the logged-in user
                var user = await _userManager.GetUserAsync(User);

                // Query the database to find the customer with the matching email
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    var cartedItems = _productRepo.GetCartItems(customer.CustomerId);
                    var viewModel = cartedItems.Select(p => new CartItemViewModel
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
                        Qty = p.Qty // Include the quantity in the view model
                    });
                    return View(viewModel);
                }
            }

            return RedirectToAction("Login", "Account");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId)
        {

            if (User.Identity.IsAuthenticated)
            {
                // Get the IdentityUser object for the logged-in user
                var user = await _userManager.GetUserAsync(User);

                // Query the database to find the customer with the matching email
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    // Convert CustomerId from long to int
                    int customerId = customer.CustomerId;

                    // Add the product to the customer's cart
                    _productRepo.AddToCart(customerId, productId);

                    // Redirect to the cart page
                    return RedirectToAction("Cart");
                }
            }
            
            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                // Get the IdentityUser object for the logged-in user
                var user = await _userManager.GetUserAsync(User);

                // Query the database to find the customer with the matching email
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    // Remove the product from the customer's cart
                    _productRepo.RemoveFromCart(customer.CustomerId, productId);
                    return Ok();
                }
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> IncreaseQuantity(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    _productRepo.IncreaseQuantity(customer.CustomerId, productId);
                    return Ok();
                }
            }

            return Unauthorized();
        }

        [HttpPost]
        public async Task<IActionResult> DecreaseQuantity(int productId)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    _productRepo.DecreaseQuantity(customer.CustomerId, productId);
                    return Ok();
                }
            }

            return Unauthorized();
        }

    //Checkout Stuff
        public async Task<IActionResult> Checkout()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    var cartedItems = _productRepo.GetCartItems(customer.CustomerId);

                    var checkoutViewModel = new CheckoutViewModel
                    {
                        CartItems = cartedItems,
                        TotalAmount = cartedItems.Sum(item => item.Price.Value * item.Qty)
                    };

                    return View(checkoutViewModel);
                }
            }

            return RedirectToAction("Login", "Account");
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    var cartedItems = _productRepo.GetCartItems(customer.CustomerId);

                    var order = new Order
                    {
                        CustomerId = customer.CustomerId,
                        Date = DateOnly.FromDateTime(DateTime.Now),
                        DayOfWeek = DateTime.Now.DayOfWeek.ToString(),
                        Time = (byte)DateTime.Now.Hour,
                        EntryMode = "Online",
                        Amount = (double?)cartedItems.Sum(item => item.Price.Value * item.Qty),
                        TypeOfTransaction = "Purchase",
                        CountryOfTransaction = "USA", // Set the appropriate country
                        ShippingAddress = "", // Set the shipping address
                        Bank = "", // Set the bank information
                        TypeOfCard = "", // Set the type of card
                        Fraud = 0 // Set the fraud flag
                    };

                    _context.Orders.Add(order);
                    _context.SaveChanges();

                    // Clear the user's cart after placing the order
                    foreach (var item in cartedItems)
                    {
                        _productRepo.RemoveFromCart(customer.CustomerId, item.ProductId);
                    }

                    return RedirectToAction("OrderSuccess");
                }
            }

            return RedirectToAction("Login", "Account");
        }

        public IActionResult OrderSuccess()
        {
            return View();
        }









        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Register()
        {
            return View();
        }

        public IActionResult About()
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
