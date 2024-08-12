using ECommerceApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace ECommerceApp.Data
{
    public static class DbInitializer
    {
        public static void Initialize(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                context.Database.EnsureCreated();

                // Kiểm tra nếu đã có dữ liệu mẫu thì không thêm nữa
                if (context.Products.Any())
                {
                    return;   // DB đã được khởi tạo
                }

                // Thêm dữ liệu mẫu liên quan đến đông trùng hạ thảo
                var products = new Product[]
                {
                    new Product { Name = "Đông Trùng Hạ Thảo Khô", Description = "Sản phẩm đông trùng hạ thảo khô chất lượng cao", Price = 150.00M, ImageUrl = "/assets/dongtrunghathao1.jpg", Rating = 5 },
                    new Product { Name = "Đông Trùng Hạ Thảo Tươi", Description = "Sản phẩm đông trùng hạ thảo tươi nguyên chất", Price = 200.00M, ImageUrl = "/assets/dongtrunghathao2.jpg", Rating = 4 },
                    new Product { Name = "Viên Nang Đông Trùng Hạ Thảo", Description = "Viên nang đông trùng hạ thảo dễ dàng sử dụng", Price = 120.00M, ImageUrl = "/assets/dongtrunghathao3.jpg", Rating = 4 },
                    new Product { Name = "Trà Đông Trùng Hạ Thảo", Description = "Trà đông trùng hạ thảo thơm ngon bổ dưỡng", Price = 50.00M, ImageUrl = "/assets/dongtrunghathao4.jpg", Rating = 5 },
                    new Product { Name = "Bột Đông Trùng Hạ Thảo", Description = "Bột đông trùng hạ thảo dễ dàng pha chế", Price = 100.00M, ImageUrl = "/assets/dongtrunghathao5.jpg", Rating = 4 }
                };

                foreach (var p in products)
                {
                    context.Products.Add(p);
                }
                context.SaveChanges();
            }
        }
    }
}
