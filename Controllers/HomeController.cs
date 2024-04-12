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
using Microsoft.ML;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.AspNetCore.Http;

namespace _2AuthenticAPP.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductRepository _productRepo;
        private readonly BorchardtDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        private readonly InferenceSession _inferenceSession;


        public HomeController(IProductRepository productRepo, BorchardtDbContext context, UserManager<IdentityUser> userManager) 
        {
            _productRepo = productRepo;
            _context = context;
            _userManager = userManager;


            // Initialize the InferenceSession here;
            _inferenceSession = new InferenceSession("GradientBoostingClassifier_model.onnx");
        }

        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 9, string category = null, int? minParts = null, int? maxParts = null, decimal? minPrice = null, decimal? maxPrice = null, string primaryColor = null, string secondaryColor = null)
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
                        .Average(li => (int?)li.Rating),
                    Category = p.Category,
                    NumParts = p.NumParts,
                    PrimaryColor = p.PrimaryColor,
                    SecondaryColor = p.SecondaryColor
                });

            // Apply filters
            if (!string.IsNullOrEmpty(category))
            {
                productsQuery = productsQuery.Where(p => p.Category.Contains(category));
            }

            if (minParts.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.NumParts >= minParts.Value);
            }

            if (maxParts.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.NumParts <= maxParts.Value);
            }

            if (minPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.Price <= maxPrice.Value);
            }

            if (!string.IsNullOrEmpty(primaryColor))
            {
                productsQuery = productsQuery.Where(p => p.PrimaryColor == primaryColor);
            }

            if (!string.IsNullOrEmpty(secondaryColor))
            {
                productsQuery = productsQuery.Where(p => p.SecondaryColor == secondaryColor);
            }

            // Pagination logic
            var paginatedProducts = await PaginatedList<ProductViewModel>.CreateAsync(productsQuery, pageNumber, pageSize);

            // Set ViewBag values for selected filter options
            ViewBag.SelectedCategory = category;
            ViewBag.SelectedMinParts = minParts;
            ViewBag.SelectedMaxParts = maxParts;
            ViewBag.SelectedMinPrice = minPrice;
            ViewBag.SelectedMaxPrice = maxPrice;
            ViewBag.SelectedPrimaryColor = primaryColor;
            ViewBag.SelectedSecondaryColor = secondaryColor;

            // Fetch and process categories
            var allCategories = await _context.Products
                .Select(p => p.Category)
                .ToListAsync();

            var uniqueCategories = allCategories
                .SelectMany(c => c.Split('-'))
                .Select(c => c.Trim())
                .Distinct()
                .ToList();

            ViewBag.Categories = uniqueCategories;

            // Fetch and process primary colors
            var allPrimaryColors = await _context.Products
                .Select(p => p.PrimaryColor)
                .Distinct()
                .ToListAsync();

            ViewBag.PrimaryColors = allPrimaryColors;

            // Fetch and process secondary colors
            var allSecondaryColors = await _context.Products
                .Select(p => p.SecondaryColor)
                .Distinct()
                .ToListAsync();

            ViewBag.SecondaryColors = allSecondaryColors;

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
        [Authorize]
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
        [Authorize]
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

        [Authorize]
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
                        Date = DateTime.Now,
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


        //Order History Stuff
        [Authorize]
        public async Task<IActionResult> OrderHistory()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    var orders = await _context.Orders
                        .Where(o => o.CustomerId == customer.CustomerId)
                        .ToListAsync();

                    return View(orders);
                }
            }

            return RedirectToAction("Login", "Account");
        }
        [Authorize]
        public async Task<IActionResult> OrderDetails(int id)
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Email == user.Email);

                if (customer != null)
                {
                    var order = await _context.Orders
                        .Include(o => o.LineItems)
                            .ThenInclude(li => li.Product)
                        .FirstOrDefaultAsync(o => o.TransactionId == id && o.CustomerId == customer.CustomerId);

                    if (order != null)
                    {
                        return View(order);
                    }
                }
            }

            return RedirectToAction("Login", "Account");
        }

        //CRUD PRODUCTS
        public IActionResult AddProduct()
        {
            return View();
        }

        [HttpPost]
        public IActionResult AddProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Add(product);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(product);
        }

        public IActionResult UpdateProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult UpdateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                _context.Products.Update(product);
                _context.SaveChanges();
                return RedirectToAction("Details", new { id = product.ProductId });
            }
            return View(product);
        }

        public IActionResult DeleteProduct(int id)
        {
            var product = _context.Products.Find(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        [HttpPost]
        public IActionResult DeleteConfirmed(int productId)
        {
            var product = _context.Products.Find(productId);
            if (product != null)
            {
                _context.Products.Remove(product);
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
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

        // Perdiction Realm

       [HttpPost]
        public IActionResult Predict(int customerId, DateTime date, string dayOfWeek, int time, string entryMode, float amount, string typeOfTrans, string countryOfTrans, string shippingAddress, string bank, string typeOfCard)
        {
            // Dictionary mapping the numeric prediction to string
            var class_type_dict = new Dictionary<int, string>
            {
                { 0, "Valid" },
                { 1, "Fraud" }
            };

            // Calculate days since January 1, 2023

            var january1_2023 = new DateTime(2023, 1, 1);

            var daySinceJan12023 = Math.Abs((date - january1_2023).Days);

            var input = new List<float>
            {
                (float)customerId,
                (float)daySinceJan12023,
                (float)time,
                (float)amount,

                // Check the dummy coded data
                // Deal with week day
                dayOfWeek == "Mon" ? 1 : 0,
                dayOfWeek == "Sat" ? 1 : 0,
                dayOfWeek == "Sun" ? 1 : 0,
                dayOfWeek == "Thu" ? 1 : 0,
                dayOfWeek == "Tue" ? 1 : 0,
                dayOfWeek == "Wed" ? 1 : 0,

                //Entry Mode
                entryMode == "PIN" ? 1 : 0,
                entryMode == "Tap" ? 1 : 0,

                // Transaction Type
                typeOfTrans == "Online" ? 1 : 0,
                typeOfTrans == "POS" ? 1 : 0,

                // Country of Transaction
                countryOfTrans == "Indian" ? 1 : 0,
                countryOfTrans == "Russia" ? 1 : 0,
                countryOfTrans == "USA" ? 1 : 0,
                countryOfTrans == "United Kingdom" ? 1 : 0,

                //Shipping Address
                (shippingAddress ?? countryOfTrans) == "India" ? 1 : 0,
                (shippingAddress ?? countryOfTrans) == "Russia" ? 1 : 0,
                (shippingAddress ?? countryOfTrans) == "USA" ? 1 : 0,
                (shippingAddress ?? countryOfTrans) == "United Kingdom" ? 1 : 0,

                // Bank
                bank == "HSBC" ? 1 : 0,
                bank == "Halifax" ? 1 : 0,
                bank == "Lloyds" ? 1 : 0,
                bank == "Metro" ? 1 : 0,
                bank == "Monzo" ? 1 : 0,
                bank == "RBS" ? 1 : 0,

                // Type of Card
                typeOfCard == "Visa" ? 1 : 0,
            };
            var inputTensor = new DenseTensor<float>(input.ToArray(), new[] { 1, input.Count });

            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("float_input", inputTensor)
            };


            using (var results = _inferenceSession.Run(inputs)) // makes the prediction with the inputs from the form (i.e. class_type 1-7)
            {
                var prediction = results.FirstOrDefault(item => item.Name == "output_label")?.AsTensor<long>().ToArray();
                if (prediction != null && prediction.Length > 0)
                {
                    // Use the prediction to get the animal type from the dictionary
                    var animalType = class_type_dict.GetValueOrDefault((int)prediction[0], "Unknown");
                    ViewBag.Prediction = animalType;
                }
                else
                {
                    ViewBag.Prediction = "Error: Unable to make a prediction.";
                }
            }

            return View("Index");
        }
    }
}
