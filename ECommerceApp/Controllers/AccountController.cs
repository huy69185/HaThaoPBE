using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using ECommerceApp.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ECommerceApp.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace ECommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                    if (!roleResult.Succeeded)
                    {
                        foreach (var error in roleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }

                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    else if (roles.Contains("Employee"))
                    {
                        return RedirectToAction("Index", "Employee");
                    }
                    else
                    {
                        return RedirectToAction("Index", "Home");
                    }
                }
                if (result.IsLockedOut)
                {
                    return View("Lockout");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                }
            }
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            var claims = await _context.UserClaims.Where(c => c.UserId == user.Id).ToListAsync();
            var userFullName = claims.FirstOrDefault(c => c.ClaimType == "UserFullName")?.ClaimValue;
            var imgUrl = claims.FirstOrDefault(c => c.ClaimType == "ImgUrl")?.ClaimValue;

            var model = new ProfileViewModel
            {
                UserFullName = userFullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ImgUrl = imgUrl
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            if (!string.IsNullOrEmpty(model.Password) && model.Password == model.ConfirmPassword)
            {
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.Password);
                if (!result.Succeeded)
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(model);
                }
            }

            var claims = await _context.UserClaims.Where(c => c.UserId == user.Id).ToListAsync();
            var userFullNameClaim = claims.FirstOrDefault(c => c.ClaimType == "UserFullName");
            var imgUrlClaim = claims.FirstOrDefault(c => c.ClaimType == "ImgUrl");

            if (!string.IsNullOrEmpty(model.UserFullName))
            {
                if (userFullNameClaim != null)
                {
                    _context.UserClaims.Remove(userFullNameClaim);
                }
                _context.UserClaims.Add(new IdentityUserClaim<string>
                {
                    UserId = user.Id,
                    ClaimType = "UserFullName",
                    ClaimValue = model.UserFullName
                });
            }

            if (!string.IsNullOrEmpty(model.ImgUrl))
            {
                if (imgUrlClaim != null)
                {
                    _context.UserClaims.Remove(imgUrlClaim);
                }
                _context.UserClaims.Add(new IdentityUserClaim<string>
                {
                    UserId = user.Id,
                    ClaimType = "ImgUrl",
                    ClaimValue = model.ImgUrl
                });
            }

            _context.Users.Update(user); // Cập nhật trực tiếp thông tin user
            await _context.SaveChangesAsync(); // Lưu toàn bộ thay đổi vào DB

            ViewBag.SuccessMessage = "Profile updated successfully.";
            return View(model);
        }

    }
}
