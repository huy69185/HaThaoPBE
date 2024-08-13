using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using ECommerceApp.Data;
using Newtonsoft.Json;

namespace ECommerceApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Thêm dịch vụ cho Entity Framework Core với SQLite
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Thêm dịch vụ cho Identity
            builder.Services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = false)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Thêm dịch vụ cho MVC và API
            builder.Services.AddControllersWithViews();
            builder.Services.AddControllers();  // Thêm dòng này để hỗ trợ API

            // Thêm dịch vụ cho Session
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            var app = builder.Build();

            // Cấu hình pipeline HTTP
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            // Sử dụng Session middleware
            app.UseSession();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllers();  // Thêm dòng này để hỗ trợ API

            // Gọi phương thức khởi tạo DB
            DbInitializer.Initialize(app);

            app.Run();
        }
    }
}
