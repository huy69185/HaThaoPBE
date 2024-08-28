using ECommerceApp.Data;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ECommerceApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Select(o => new
                {
                    Order = o,
                    UserEmail = _context.Users
                                .Where(u => u.Id == o.UserId)
                                .Select(u => u.Email)
                                .FirstOrDefault()
                })
                .ToListAsync();

            var customers = await _userManager.Users.ToListAsync();
            var employees = await _userManager.Users.ToListAsync();

            var customerDetails = await GetUsersWithClaims(customers, "Customer");
            var employeeDetails = await GetUsersWithClaims(employees, "Employee");

            var revenueData = await _context.Orders
                .Where(o => o.PaymentStatus == "Đã thanh toán")
                .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRevenue = g.Sum(o => o.OrderItems.Sum(oi => (double)oi.Price * oi.Quantity))
                })
                .ToListAsync();

            var registrationData = await _context.UserMetadata
                .GroupBy(um => new { um.RegisterDate.Year, um.RegisterDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    TotalRegistrations = g.Count()
                })
                .ToListAsync();

            var orderTuples = orders.Select(o => Tuple.Create(o.Order, o.UserEmail)).ToList();

            var viewModel = new AdminDashboardViewModel
            {
                OrderTuples = orderTuples,
                Customers = customerDetails,
                Employees = employeeDetails,
                RevenueData = revenueData,
                RegistrationData = registrationData
            };

            return View(viewModel);
        }

        private async Task<List<IdentityUserWithClaims>> GetUsersWithClaims(IEnumerable<IdentityUser> users, string role)
        {
            var result = new List<IdentityUserWithClaims>();

            foreach (var user in users)
            {
                if (await _userManager.IsInRoleAsync(user, role))
                {
                    var claims = await _userManager.GetClaimsAsync(user);
                    var userFullName = claims.FirstOrDefault(c => c.Type == "UserFullName")?.Value;
                    var imgUrl = claims.FirstOrDefault(c => c.Type == "ImgUrl")?.Value;

                    result.Add(new IdentityUserWithClaims
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserFullName = userFullName,
                        ImgUrl = imgUrl
                    });
                }
            }

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount(string email, string password, string role)
        {
            var user = new IdentityUser { UserName = email, Email = email };
            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                return RedirectToAction("Index");
            }

            return View("Index");
        }
    }

    public class IdentityUserWithClaims
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string UserFullName { get; set; }
        public string ImgUrl { get; set; }
    }
}
