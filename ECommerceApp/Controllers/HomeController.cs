using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using System.Linq;

namespace ECommerceApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var products = _context.Products.ToList();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
