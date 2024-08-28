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
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace ECommerceApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AccountController> _logger;
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext context,
            ILogger<AccountController> logger,
            IWebHostEnvironment hostingEnvironment,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _context = context;
            _logger = logger;
            _hostingEnvironment = hostingEnvironment;
            _emailSender = emailSender;
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
                try
                {
                    // Tạo người dùng mới nhưng không xác nhận email
                    var user = new IdentityUser { UserName = model.Email, Email = model.Email };
                    var result = await _userManager.CreateAsync(user, model.Password);

                    if (result.Succeeded)
                    {
                        // Thêm người dùng vào vai trò "Customer"
                        await _userManager.AddToRoleAsync(user, "Customer");

                        // Tạo mã xác nhận 6 ký tự
                        var verificationCode = GenerateVerificationCode(6);
                        var timeCreated = DateTime.UtcNow;

                        // Lưu mã xác nhận và thời gian tạo mã vào Claims của người dùng
                        await _userManager.AddClaimAsync(user, new Claim("VerificationCode", verificationCode));
                        await _userManager.AddClaimAsync(user, new Claim("TimeCreated", timeCreated.ToString("o")));

                        // Gửi mã xác nhận qua email
                        await _emailSender.SendEmailAsync(
                            model.Email,
                            "Email Verification Code",
                            $"Your verification code is {verificationCode}. This code is valid for 60 seconds.");

                        // Lưu ngày đăng ký vào bảng UserMetadata
                        var metadata = new UserMetadata
                        {
                            Id = user.Id,
                            RegisterDate = DateTime.UtcNow // Lưu ngày hiện tại làm ngày đăng ký
                        };
                        _context.UserMetadata.Add(metadata);
                        await _context.SaveChangesAsync();

                        return RedirectToAction("VerifyEmail", "Mail", new { email = user.Email });
                    }
                    else
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An unexpected error occurred. Please try again later.");
                }
            }

            return View(model);
        }


        private string GenerateVerificationCode(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
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

            // Lấy thông tin từ Claims
            var claims = await _userManager.GetClaimsAsync(user);
            var userFullName = claims.FirstOrDefault(c => c.Type == "UserFullName")?.Value;
            var imgUrl = claims.FirstOrDefault(c => c.Type == "ImgUrl")?.Value;

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

            // Cập nhật Email nếu nó khác với Email hiện tại
            if (!string.IsNullOrEmpty(model.Email) && model.Email != user.Email)
            {
                user.Email = model.Email;
                user.UserName = model.Email;
            }

            // Cập nhật Số điện thoại nếu nó khác với số hiện tại
            if (!string.IsNullOrEmpty(model.PhoneNumber) && model.PhoneNumber != user.PhoneNumber)
            {
                user.PhoneNumber = model.PhoneNumber;
            }

            // Cập nhật mật khẩu nếu người dùng nhập mật khẩu mới và xác nhận mật khẩu trùng khớp
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

            // Xử lý upload ảnh đại diện nếu người dùng đã tải ảnh mới lên
            if (model.ProfileImageFile != null && model.ProfileImageFile.Length > 0)
            {
                var uploads = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfileImageFile.FileName);

                if (!Directory.Exists(uploads))
                {
                    Directory.CreateDirectory(uploads);
                }

                var filePath = Path.Combine(uploads, fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.ProfileImageFile.CopyToAsync(fileStream);
                }

                // Cập nhật URL của ảnh mới vào model
                model.ImgUrl = "/uploads/" + fileName;
            }
            else
            {
                // Nếu không có ảnh mới, giữ nguyên giá trị ImgUrl cũ
                var claims = await _userManager.GetClaimsAsync(user);
                model.ImgUrl = claims.FirstOrDefault(c => c.Type == "ImgUrl")?.Value;
            }

            // Cập nhật Claims cho Tên đầy đủ và Ảnh đại diện
            var claimsList = await _userManager.GetClaimsAsync(user);
            var userFullNameClaim = claimsList.FirstOrDefault(c => c.Type == "UserFullName");
            var imgUrlClaim = claimsList.FirstOrDefault(c => c.Type == "ImgUrl");

            if (!string.IsNullOrEmpty(model.UserFullName))
            {
                if (userFullNameClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, userFullNameClaim);
                }
                await _userManager.AddClaimAsync(user, new Claim("UserFullName", model.UserFullName));
            }

            if (!string.IsNullOrEmpty(model.ImgUrl))
            {
                if (imgUrlClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, imgUrlClaim);
                }
                await _userManager.AddClaimAsync(user, new Claim("ImgUrl", model.ImgUrl));
            }

            await _context.SaveChangesAsync(); // Lưu vào DB

            ViewBag.SuccessMessage = "Profile updated successfully.";
            return View(model);
        }

    }
}