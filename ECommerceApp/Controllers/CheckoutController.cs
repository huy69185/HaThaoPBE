using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ECommerceApp.Models;
using ECommerceApp.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Helpers;
using System;
using System.Security.Claims;

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

        // Tạo tên cookie dựa trên UserID
        private string GetCartCookieName()
        {
            if (!User.Identity.IsAuthenticated)
            {
                return null;
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return $"cart_{userId}";
        }

        // Lấy các mục trong giỏ hàng
        private List<CartItem> GetCartItems()
        {
            var cartCookieName = GetCartCookieName();
            if (cartCookieName == null) return new List<CartItem>();

            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>(cartCookieName) ?? new List<CartItem>();
            return cart;
        }

        [HttpGet]
        public IActionResult Index(List<int> SelectedProductIds)
        {
            var cart = GetCartItems();
            var selectedItems = cart.Where(c => SelectedProductIds.Contains(c.Product.Id)).ToList();

            if (!selectedItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var viewModel = new CheckoutViewModel
            {
                CartItems = selectedItems
            };
            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(CheckoutViewModel viewModel)
        {
            var cart = GetCartItems();
            var selectedItems = cart.Where(c => viewModel.SelectedProductIds.Contains(c.Product.Id)).ToList();

            if (!selectedItems.Any())
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
                PaymentStatus = "Đã thanh toán",
                UserId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                OrderItems = selectedItems.Select(item => new OrderItem
                {
                    ProductId = item.Product.Id,
                    Quantity = item.Quantity,
                    Price = item.Product.Price
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Remove the selected items from the cart
            var updatedCart = cart.Except(selectedItems).ToList();
            SaveCartItems(updatedCart);

            if (viewModel.PaymentMethod == "Chuyển khoản")
            {
                var qrCodeUrl = GenerateVietQrUrl(order, out string accountNo, out string accountName, out int amount, out string addInfo);
                return View("GenerateQRCode", new GenerateQRCodeViewModel { QRCodeUrl = qrCodeUrl, Order = order, AccountNo = accountNo, AccountName = accountName, Amount = amount, AddInfo = addInfo });
            }

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }
        // Add this method to your CheckoutController
        private void SaveCartItems(List<CartItem> cart)
        {
            var cartCookieName = GetCartCookieName();
            if (cartCookieName != null)
            {
                HttpContext.Response.SetObjectAsJsonCookie(cartCookieName, cart);
            }
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
                TransactionDate = DateTime.Now
            };

            _context.Transactions.Add(transaction);

            // Cập nhật trạng thái đơn hàng và trạng thái thanh toán
            order.Status = "Chờ xác nhận";
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
