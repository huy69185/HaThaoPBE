using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceApp.Controllers
{
    public class CommentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CommentController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Details(int id)
        {
            var product = await _context.Products
                .Include(p => p.Votes) // Include Votes
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(int productId, string comment, double rating)
        {
            var vote = new Vote
            {
                ProductID = productId,
                Comment = comment,
                Rating = rating
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

            return RedirectToAction("Details", new { id = productId });
        }
    }
}
