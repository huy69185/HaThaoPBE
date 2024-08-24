using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult ProductDetails(int id)
        {
            var product = _context.Products
                .Include(p => p.Votes)
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var canComment = _context.Orders
                .Include(o => o.OrderItems)
                .Any(o => o.UserId == userId &&
                          o.OrderItems.Any(oi => oi.ProductId == id) &&
                          o.Status == "Đã giao");

            ViewData["CanComment"] = canComment;
            ViewData["Votes"] = product.Votes;

            var randomProducts = _context.Products
                .AsEnumerable()
                .OrderBy(p => System.Guid.NewGuid())
                .Take(4)
                .ToList();

            ViewData["RandomProducts"] = randomProducts;

            return View(product);
        }

        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string[] filterCategory, int minPrice = 0, int maxPrice = 10000000)
        {
            var products = from p in _context.Products select p;

            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            if (filterCategory != null && filterCategory.Length > 0)
            {
                products = products.Where(p => filterCategory.Contains(p.Category));
            }

            products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

            switch (sortOrder)
            {
                case "name_asc":
                    products = products.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    products = products.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                case "rating_asc":
                    products = products.OrderBy(p => p.AverageRating);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.AverageRating);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentCategory"] = filterCategory;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;

            return View(await products.ToListAsync());
        }
    }
}
