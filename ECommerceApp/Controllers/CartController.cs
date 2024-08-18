using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Helpers;
using ECommerceApp.Models;
using System.Collections.Generic;
using System.Linq;
using ECommerceApp.Data;

namespace ECommerceApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();
            return View(cart);
        }

        [HttpPost]
        public IActionResult AddToCart(int id, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.SingleOrDefault(p => p.Id == id);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tồn tại.";
                return RedirectToAction("Index");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.SingleOrDefault(c => c.Product.Id == id);
            if (cartItem == null)
            {
                cart.Add(new CartItem { Product = product, Quantity = quantity });
            }
            else
            {
                cartItem.Quantity += quantity;
            }

            HttpContext.Response.SetObjectAsJsonCookie("cart", cart);
            TempData["Success"] = "Sản phẩm đã được thêm vào giỏ hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng không hợp lệ.";
                return RedirectToAction("Index");
            }

            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.SingleOrDefault(c => c.Product.Id == id);
            if (cartItem != null)
            {
                cartItem.Quantity = quantity;
            }
            else
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
            }

            HttpContext.Response.SetObjectAsJsonCookie("cart", cart);
            TempData["Success"] = "Cập nhật giỏ hàng thành công.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.SingleOrDefault(c => c.Product.Id == id);
            if (cartItem != null)
            {
                cart.Remove(cartItem);
                TempData["Success"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            }
            else
            {
                TempData["Error"] = "Sản phẩm không tồn tại trong giỏ hàng.";
            }

            HttpContext.Response.SetObjectAsJsonCookie("cart", cart);
            return RedirectToAction("Index");
        }
    }
}
