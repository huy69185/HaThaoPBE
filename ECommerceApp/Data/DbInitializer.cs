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

                // Check if any products exist, if not, add seed data
                if (!context.Products.Any())
                {
                    // Seed data with the additional fields
                    var products = new Product[]
                    {
                        new Product
                        {
                            Name = "Xà Phòng Lifebuoy",
                            Description = "Xà phòng diệt khuẩn Lifebuoy",
                            Price = 15000.00M,
                            ImageUrl = "/assets/soap1.jpg",
                            Rating = 5,
                            Quantity = 100,
                            Category = "Soap",
                            Brand = "Lifebuoy",
                            ManufacturedIn = "Vietnam",
                            Fragrance = "Hương bạc hà",
                            Usage = "Sử dụng hàng ngày",
                            Weight = "100g",
                            Ingredients = "Sodium Palmate, Sodium Palm Kernelate, Water, Glycerin, Fragrance",
                            Storage = "Tránh ánh nắng trực tiếp, bảo quản nơi khô ráo"
                        },
                        new Product
                        {
                            Name = "Bột Giặt Omo",
                            Description = "Bột giặt Omo tẩy sạch vết bẩn",
                            Price = 20000.00M,
                            ImageUrl = "/assets/detergent1.jpg",
                            Rating = 4,
                            Quantity = 150,
                            Category = "Detergent",
                            Brand = "Omo",
                            ManufacturedIn = "Vietnam",
                            Fragrance = "Hương sữa cá ngựa",
                            Usage = "Giặt tay, Giặt máy cửa trước và cửa trên",
                            Weight = "5kg",
                            Ingredients = "Sodium Lauryl Ether Sulphate, Sodium Lauryl sulfate, Hydroxy Propyl Methyl Cellulose, hương, nước và một số phụ gia khác.",
                            Storage = "Tránh ánh nắng trực tiếp, nhiệt độ và độ ẩm cao, đậy nắp sau khi sử dụng."
                        },
                        new Product
                        {
                            Name = "Nước Giặt Ariel",
                            Description = "Nước giặt Ariel thơm mát",
                            Price = 25000.00M,
                            ImageUrl = "/assets/detergent2.jpg",
                            Rating = 4,
                            Quantity = 200,
                            Category = "Detergent",
                            Brand = "Ariel",
                            ManufacturedIn = "Vietnam",
                            Fragrance = "Hương hoa nhài",
                            Usage = "Giặt tay và máy giặt",
                            Weight = "3L",
                            Ingredients = "Water, Linear Alkylbenzene Sulfonate, Sodium Laureth Sulfate, Alcohol Ethoxylate, Fragrance",
                            Storage = "Đậy nắp sau khi sử dụng, tránh nhiệt độ cao"
                        },
                        new Product
                        {
                            Name = "Xà Phòng Coast",
                            Description = "Xà phòng Coast hương biển",
                            Price = 12000.00M,
                            ImageUrl = "/assets/soap2.jpg",
                            Rating = 5,
                            Quantity = 120,
                            Category = "Soap",
                            Brand = "Coast",
                            ManufacturedIn = "Vietnam",
                            Fragrance = "Hương biển",
                            Usage = "Sử dụng hàng ngày",
                            Weight = "125g",
                            Ingredients = "Sodium Tallowate, Sodium Cocoate, Water, Glycerin, Fragrance",
                            Storage = "Bảo quản nơi khô ráo, thoáng mát"
                        },
                        new Product
                        {
                            Name = "Bột Giặt Tide",
                            Description = "Bột giặt Tide trắng sáng",
                            Price = 22000.00M,
                            ImageUrl = "/assets/detergent3.jpg",
                            Rating = 4,
                            Quantity = 80,
                            Category = "Detergent",
                            Brand = "Tide",
                            ManufacturedIn = "Vietnam",
                            Fragrance = "Hương thơm nhẹ nhàng",
                            Usage = "Giặt tay và giặt máy",
                            Weight = "4kg",
                            Ingredients = "Sodium Carbonate, Sodium Sulfate, Linear Alkylbenzene Sulfonate, Fragrance",
                            Storage = "Tránh ánh nắng trực tiếp, bảo quản nơi khô ráo"
                        }
                    };

                    foreach (var p in products)
                    {
                        context.Products.Add(p);
                    }
                    context.SaveChanges();
                }

                // Create roles if they do not exist
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

                // Create default Admin account if not already created
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

                // Create default Employee account if not already created
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
