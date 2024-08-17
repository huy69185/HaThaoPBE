using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Models
{
    public class ProfileViewModel
    {
        public string UserFullName { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        // Thuộc tính để lưu đường dẫn ảnh đại diện
        [Display(Name = "Profile Image")]
        public string ImgUrl { get; set; }

        public IFormFile ProfileImageFile { get; set; } // Thêm thuộc tính để upload file ảnh đại diện
    }


}
