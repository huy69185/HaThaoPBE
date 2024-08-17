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
            ViewData["Votes"] = product.Votes; // Truyền danh sách đánh giá vào View

            return View(product);
        }
        public async Task<IActionResult> Index(String searchTerm)
        {
            var products = from p in _context.Products select p;
            if (!string.IsNullOrEmpty(searchTerm))
            {
                // Tính năng duyệt theo tên sản phẩm chỗ nào có tên thì hiện ra
                var searchTermLower = searchTerm.ToLower();
                products = products.Where(p => p.Name.ToLower().Contains(searchTermLower));
            }
            return View(products);
        }

   
    }
}
