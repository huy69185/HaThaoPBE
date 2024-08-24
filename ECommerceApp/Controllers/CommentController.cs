using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ECommerceApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int productId, string comment, double rating)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Kiểm tra xem người dùng đã mua sản phẩm và trạng thái đơn là "Đã giao" chưa
            var hasPurchased = await _context.Orders
                .Include(o => o.OrderItems)
                .AnyAsync(o => o.UserId == userId &&
                               o.OrderItems.Any(oi => oi.ProductId == productId) &&
                               o.Status == "Đã giao");

            if (!hasPurchased)
            {
                return Forbid(); // Cấm người dùng không đủ điều kiện gửi bình luận
            }

            var vote = new Vote
            {
                ProductID = productId,
                Comment = comment,
                Rating = rating,
                CustomerID = userId // Lưu CustomerId vào bảng Vote
            };

            _context.Votes.Add(vote);
            await _context.SaveChangesAsync();

            // Cập nhật lại giá trị trung bình của Rating cho sản phẩm
            var product = await _context.Products.Include(p => p.Votes).FirstOrDefaultAsync(p => p.Id == productId);
            if (product != null)
            {
                product.Rating = product.Votes.Any() ? product.Votes.Average(v => v.Rating) : 0.0;
                await _context.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString());
        }

    }
}
