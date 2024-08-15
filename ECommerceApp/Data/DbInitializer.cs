using ECommerceApp.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Threading.Tasks;

namespace ECommerceApp.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
                var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                context.Database.EnsureCreated();

                // Kiểm tra nếu đã có dữ liệu mẫu thì không thêm nữa
                if (!context.Products.Any())
                {
                    // Thêm dữ liệu mẫu cho sản phẩm xà phòng và bột giặt
                    var products = new Product[]
                    {
                        new Product { Name = "Xà Phòng Lifebuoy", Description = "Xà phòng diệt khuẩn Lifebuoy", Price = 15000.00M, ImageUrl = "/assets/soap1.jpg", Rating = 5, Quantity = 100 },
                        new Product { Name = "Bột Giặt Omo", Description = "Bột giặt Omo tẩy sạch vết bẩn", Price = 20000.00M, ImageUrl = "/assets/detergent1.jpg", Rating = 4, Quantity = 150 },
                        new Product { Name = "Nước Giặt Ariel", Description = "Nước giặt Ariel thơm mát", Price = 25000.00M, ImageUrl = "/assets/detergent2.jpg", Rating = 4, Quantity = 200 },
                        new Product { Name = "Xà Phòng Coast", Description = "Xà phòng Coast hương biển", Price = 12000.00M, ImageUrl = "/assets/soap2.jpg", Rating = 5, Quantity = 120 },
                        new Product { Name = "Bột Giặt Tide", Description = "Bột giặt Tide trắng sáng", Price = 22000.00M, ImageUrl = "/assets/detergent3.jpg", Rating = 4, Quantity = 80 }
                    };

                    foreach (var p in products)
                    {
                        context.Products.Add(p);
                    }
                    context.SaveChanges();
                }

                // Tạo các vai trò (Roles) cho hệ thống
                string[] roleNames = { "Admin", "Employee" };
                IdentityResult roleResult;

                foreach (var roleName in roleNames)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                    }
                }

                // Tạo tài khoản Admin mặc định
                if (context.Users.Any(u => u.UserName == "admin@admin.com") == false)
                {
                    var user = new IdentityUser
                    {
                        UserName = "admin@admin.com",
                        Email = "admin@admin.com"
                    };

                    string userPassword = "Admin@123";
                    var adminUser = await userManager.CreateAsync(user, userPassword);

                    if (adminUser.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Admin");
                    }
                }

                // Tạo tài khoản Employee mặc định
                if (context.Users.Any(u => u.UserName == "employee@employee.com") == false)
                {
                    var user = new IdentityUser
                    {
                        UserName = "employee@employee.com",
                        Email = "employee@employee.com"
                    };

                    string userPassword = "Employee@123";
                    var employeeUser = await userManager.CreateAsync(user, userPassword);

                    if (employeeUser.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "Employee");
                    }
                }
            }
        }
    }
}
