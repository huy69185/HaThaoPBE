using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ECommerceApp.Models;
using ECommerceApp.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using ECommerceApp.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ECommerceApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(ApplicationDbContext context, IConfiguration configuration, ILogger<CheckoutController> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();
            var viewModel = new CheckoutViewModel
            {
                CartItems = cart
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel viewModel)
        {
            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();

            if (!cart.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var order = new Order
            {
                RecipientName = viewModel.RecipientName,
                RecipientPhone = viewModel.RecipientPhone,
                Address = viewModel.Address,
                PaymentMethod = viewModel.PaymentMethod,
                Status = "Chờ xác nhận",
                OrderItems = cart.Select(item => new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            _context.SaveChanges();

            HttpContext.Response.SetObjectAsJsonCookie("cart", new List<CartItem>());

            if (viewModel.PaymentMethod == "Chuyển khoản")
            {
                var qrCodeUrl = GenerateVietQrUrl(order);
                return View("GenerateQRCode", new GenerateQRCodeViewModel { QRCodeUrl = qrCodeUrl, Order = order });
            }

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        private string GenerateVietQrUrl(Order order)
        {
            var accountNo = "07114018501";
            var accountName = "HUYNH NGHIEM NHAT HUY";
            var acqId = "970423";
            var amount = (int)order.OrderItems.Sum(item => item.Quantity * item.Price);
            var addInfo = $"Order_{order.Id}";

            var qrCodeUrl = $"https://api.vietqr.io/image/{acqId}-{accountNo}-6z1oqd9.jpg?accountName={accountName}&amount={amount}&addInfo={addInfo}";
            return qrCodeUrl;
        }

        [HttpGet]
        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }
    }

    public class GenerateQRCodeViewModel
    {
        public string QRCodeUrl { get; set; }
        public Order Order { get; set; }
    }
}
