using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ECommerceApp.Models;
using ECommerceApp.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Helpers;
using System;

namespace ECommerceApp.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public CheckoutController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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
                PaymentStatus = "Chưa thanh toán", // Thêm PaymentStatus ban đầu
                OrderItems = cart.Select(item => new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            HttpContext.Response.SetObjectAsJsonCookie("cart", new List<CartItem>());

            if (viewModel.PaymentMethod == "Chuyển khoản")
            {
                var qrCodeUrl = GenerateVietQrUrl(order, out string accountNo, out string accountName, out int amount, out string addInfo);
                return View("GenerateQRCode", new GenerateQRCodeViewModel { QRCodeUrl = qrCodeUrl, Order = order, AccountNo = accountNo, AccountName = accountName, Amount = amount, AddInfo = addInfo });
            }

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }

        private string GenerateVietQrUrl(Order order, out string accountNo, out string accountName, out int amount, out string addInfo)
        {
            accountNo = "07114018501";
            accountName = "HUYNH NGHIEM NHAT HUY";
            var acqId = "970423";
            amount = (int)order.OrderItems.Sum(item => item.Quantity * item.Price);
            addInfo = $"Order_{order.Id}";

            var qrCodeUrl = $"https://api.vietqr.io/image/{acqId}-{accountNo}-6z1oqd9.jpg?accountName={accountName}&amount={amount}&addInfo={addInfo}";
            return qrCodeUrl;
        }

        [HttpGet]
        public async Task<IActionResult> GenerateQRCode(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            var qrCodeUrl = GenerateVietQrUrl(order, out string accountNo, out string accountName, out int amount, out string addInfo);
            var viewModel = new GenerateQRCodeViewModel
            {
                QRCodeUrl = qrCodeUrl,
                Order = order,
                AccountNo = accountNo,
                AccountName = accountName,
                Amount = amount,
                AddInfo = addInfo
            };

            return View(viewModel);
        }

        [HttpGet]
        public async Task<IActionResult> OrderConfirmation(int orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .SingleOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpGet]
        public IActionResult CheckPaymentStatus(int orderId)
        {
            var order = _context.Orders.SingleOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            return Json(new { Status = order.PaymentStatus }); // Trả về PaymentStatus thay vì Status
        }

        [HttpGet]
        public IActionResult NotifyPayment(int orderId)  // Nhận orderId từ query string
        {
            return View(orderId); // Truyền orderId đến view
        }

        [HttpPost]
        public async Task<IActionResult> NotifyPayment(int orderId, string transactionId, string bank)
        {
            var order = await _context.Orders.SingleOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            // Lưu thông tin giao dịch vào cơ sở dữ liệu
            var transaction = new Transaction
            {
                OrderId = orderId,
                TransactionId = transactionId,
                Bank = bank,
                TransactionDate = DateTime.UtcNow
            };

            _context.Transactions.Add(transaction);

            // Cập nhật trạng thái đơn hàng và trạng thái thanh toán
            order.Status = "Đã chuyển khoản";
            order.PaymentStatus = "Đã thanh toán";
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }
    }

    public class GenerateQRCodeViewModel
    {
        public string QRCodeUrl { get; set; }
        public Order Order { get; set; }
        public string AccountNo { get; set; }
        public string AccountName { get; set; }
        public int Amount { get; set; }
        public string AddInfo { get; set; }
    }
}
