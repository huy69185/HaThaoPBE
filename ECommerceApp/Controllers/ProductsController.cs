using Microsoft.AspNetCore.Mvc;
using ECommerceApp.Models;
using System.Collections.Generic;

namespace ECommerceApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private static readonly List<Product> Products = new List<Product>
        {
            new Product { Id = 1, Name = "Product 1", Description = "Description of product 1", Price = 24.99M, ImageUrl = "/assets/product1.jpg", Rating = 4 },
            new Product { Id = 2, Name = "Product 2", Description = "Description of product 2", Price = 49.99M, ImageUrl = "/assets/product2.jpg", Rating = 3 },
            new Product { Id = 3, Name = "Product 3", Description = "Description of product 3", Price = 74.99M, ImageUrl = "/assets/product3.jpg", Rating = 5 },
        };

        [HttpGet]
        public IEnumerable<Product> Get()
        {
            return Products;
        }

        [HttpGet("{id}")]
        public ActionResult<Product> Get(int id)
        {
            var product = Products.Find(p => p.Id == id);
            if (product == null)
            {
                return NotFound();
            }
            return product;
        }
    }
}
