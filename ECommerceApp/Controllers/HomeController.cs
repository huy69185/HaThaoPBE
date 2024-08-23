using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    var products = _context.Products.ToList();
        //    return View(products);
        //}

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

            // Lấy tất cả sản phẩm và sắp xếp ngẫu nhiên trên phía client
            var randomProducts = _context.Products
                .AsEnumerable() // Chuyển đổi thành IEnumerable để thực hiện sắp xếp trên client
                .OrderBy(p => Guid.NewGuid())
                .Take(4)
                .ToList();

            ViewData["RandomProducts"] = randomProducts;

            return View(product);
        }
        public async Task<IActionResult> Index(string searchTerm, string sortOrder, string[] filterCategory, int minPrice = 0, int maxPrice = 10000000)
        {
            var products = from p in _context.Products select p;

            // Tìm kiếm theo tên sản phẩm
            if (!string.IsNullOrEmpty(searchTerm))
            {
                products = products.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));
            }

            // Lọc theo Category
            if (filterCategory != null && filterCategory.Length > 0)
            {
                products = products.Where(p => filterCategory.Contains(p.Category));
            }

            // Lọc theo khoảng giá
            products = products.Where(p => p.Price >= minPrice && p.Price <= maxPrice);

            // Sắp xếp
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
                    products = products.OrderBy(p => p.Rating);
                    break;
                case "rating_desc":
                    products = products.OrderByDescending(p => p.Rating);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            // Lưu lại các giá trị filter hiện tại để hiển thị lại trên View
            ViewData["CurrentSearch"] = searchTerm;
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CurrentCategory"] = filterCategory;
            ViewData["CurrentMinPrice"] = minPrice;
            ViewData["CurrentMaxPrice"] = maxPrice;

            return View(await products.ToListAsync());
        }

    }
}
