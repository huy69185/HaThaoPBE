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
                return NotFound();

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

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateCart(int id, int quantity)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = HttpContext.Request.GetObjectFromJsonCookie<List<CartItem>>("cart") ?? new List<CartItem>();

            var cartItem = cart.SingleOrDefault(c => c.Product.Id == id);
            if (cartItem != null)
            {
                if (quantity > 0)
                {
                    cartItem.Quantity = quantity;
                }
                else
                {
                    cart.Remove(cartItem);
                }
            }

            HttpContext.Response.SetObjectAsJsonCookie("cart", cart);

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
            }

            HttpContext.Response.SetObjectAsJsonCookie("cart", cart);

            return RedirectToAction("Index");
        }
    }
}
