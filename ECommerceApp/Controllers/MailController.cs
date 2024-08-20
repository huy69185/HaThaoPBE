using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ECommerceApp.Models;
using System.Threading.Tasks;
using System.Text.Encodings.Web;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Security.Claims;
using System;

namespace ECommerceApp.Controllers
{
    public class MailController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ILogger<MailController> _logger;
        private readonly IEmailSender _emailSender;

        public MailController(UserManager<IdentityUser> userManager,
                              SignInManager<IdentityUser> signInManager,
                              ILogger<MailController> logger,
                              IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        [HttpGet]
        public async Task<IActionResult> SendVerificationEmail(string userId, string email)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var callbackUrl = Url.Action(
                "VerifyEmail",
                "Mail",
                new { userId = user.Id, code },
                protocol: Request.Scheme);

            await _emailSender.SendEmailAsync(
                email,
                "Xác nhận email của bạn",
                $"Vui lòng xác nhận tài khoản của bạn bằng cách <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>nhấn vào đây</a>.");

            _logger.LogInformation("Email xác thực đã được gửi đến người dùng: {Email}", email);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> ResendVerificationCode(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                // Tạo lại mã xác thực 6 ký tự
                var verificationCode = GenerateVerificationCode(6);
                var timeCreated = DateTime.UtcNow;

                var claims = await _userManager.GetClaimsAsync(user);
                var verificationCodeClaim = claims.FirstOrDefault(c => c.Type == "VerificationCode");
                var timeCreatedClaim = claims.FirstOrDefault(c => c.Type == "TimeCreated");

                // Xóa các claims cũ nếu tồn tại
                if (verificationCodeClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, verificationCodeClaim);
                }
                if (timeCreatedClaim != null)
                {
                    await _userManager.RemoveClaimAsync(user, timeCreatedClaim);
                }

                // Thêm các claims mới với mã xác thực và thời gian tạo
                await _userManager.AddClaimAsync(user, new Claim("VerificationCode", verificationCode));
                await _userManager.AddClaimAsync(user, new Claim("TimeCreated", timeCreated.ToString("o")));

                // Gửi lại mã xác thực qua email
                await _emailSender.SendEmailAsync(
                    email,
                    "Gửi lại mã xác thực",
                    $"Mã xác thực mới của bạn là {verificationCode}. Mã này có hiệu lực trong 60 giây.");

                _logger.LogInformation("Mã xác thực đã được gửi lại cho người dùng: {Email}", email);

                // Chuyển hướng về trang VerifyEmail
                return RedirectToAction("VerifyEmail", "Mail", new { email = email });
            }

            return View("Error");
        }

        [HttpGet]
        public IActionResult VerifyEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return View("Error");
            }

            var model = new VerifyEmailViewModel { Email = email };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyEmail(VerifyEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var claims = await _userManager.GetClaimsAsync(user);
                    var verificationCodeClaim = claims.FirstOrDefault(c => c.Type == "VerificationCode");
                    var timeCreatedClaim = claims.FirstOrDefault(c => c.Type == "TimeCreated");

                    if (verificationCodeClaim != null && timeCreatedClaim != null)
                    {
                        var timeCreated = DateTime.Parse(timeCreatedClaim.Value);
                        var elapsedTime = DateTime.UtcNow - timeCreated;

                        if (elapsedTime.TotalSeconds <= 60)
                        {
                            if (verificationCodeClaim.Value == model.Code)
                            {
                                // Xác thực tài khoản sau khi mã xác nhận đúng
                                await _userManager.RemoveClaimAsync(user, verificationCodeClaim);
                                await _userManager.RemoveClaimAsync(user, timeCreatedClaim);
                                await _userManager.AddClaimAsync(user, new Claim("EmailConfirmed", "true"));

                                // Chuyển hướng đến trang đăng nhập
                                return RedirectToAction("Login", "Account");
                            }
                            else
                            {
                                ModelState.AddModelError("", "Mã xác thực không chính xác.");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Mã xác thực đã hết hạn. Vui lòng yêu cầu mã mới.");
                            model.IsExpired = true;
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không tìm thấy mã xác thực. Vui lòng yêu cầu mã mới.");
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không tìm thấy người dùng.");
                }
            }

            return View(model);
        }

        // Hàm tạo mã xác thực 6 ký tự ngẫu nhiên
        private string GenerateVerificationCode(int length)
        {
            var random = new Random();
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}
