using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ECommerceApp.Data;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceApp.Controllers
{
    [Authorize(Roles = "Employee")]
    public class EmployeeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User) // Bao gồm thông tin người dùng
                .ToListAsync();
            return View(orders);
        }

        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.Transactions)
                .Include(o => o.User) // Bao gồm thông tin người dùng
                .SingleOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound();
            }

            return View(order);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Validate status transition only if status is provided and different
            if (!string.IsNullOrEmpty(status) && order.Status != status)
            {
                if (IsValidStatusTransition(order.Status, status))
                {
                    order.Status = status;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return BadRequest("Trạng thái đơn hàng không hợp lệ.");
                }
            }

            return BadRequest("Không có thay đổi nào để cập nhật.");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string paymentStatus)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            // Validate payment status transition only if paymentStatus is provided and different
            if (!string.IsNullOrEmpty(paymentStatus) && order.PaymentStatus != paymentStatus)
            {
                if (IsValidPaymentStatusTransition(order.PaymentStatus, paymentStatus))
                {
                    order.PaymentStatus = paymentStatus;
                    await _context.SaveChangesAsync();
                    return Ok();
                }
                else
                {
                    return BadRequest("Trạng thái thanh toán không hợp lệ.");
                }
            }

            return BadRequest("Không có thay đổi nào để cập nhật.");
        }

        // Helper method to validate if the order status transition is allowed
        private bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            var validTransitions = new Dictionary<string, string[]>
            {
                { "Chờ xác nhận", new[] { "Đã xác nhận" } },
                { "Đã xác nhận", new[] { "Đang giao" } },
                { "Đang giao", new[] { "Đã giao" } }
            };
            if (newStatus == "Bùng hàng")
            {
                return true;
            }
            return validTransitions.ContainsKey(currentStatus) &&
                   validTransitions[currentStatus].Contains(newStatus);
        }

        // Helper method to validate if the payment status transition is allowed
        private bool IsValidPaymentStatusTransition(string currentPaymentStatus, string newPaymentStatus)
        {
            // Only allow transitioning from "Chưa thanh toán" to "Đã thanh toán"
            if (currentPaymentStatus == "Chưa thanh toán" && newPaymentStatus == "Đã thanh toán")
            {
                return true;
            }

            // Do not allow reverting back to "Chưa thanh toán"
            if (currentPaymentStatus == "Đã thanh toán" && newPaymentStatus == "Chưa thanh toán")
            {
                return false;
            }

            // If the status is already "Đã thanh toán", do nothing (ignore)
            return currentPaymentStatus == newPaymentStatus;
        }
    }
}
