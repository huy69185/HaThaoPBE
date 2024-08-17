using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Models
{
    public class ProfileViewModel
    {
        public string UserFullName { get; set; }
        public string ImgUrl { get; set; }

        [EmailAddress(ErrorMessage = "Invalid Email Address.")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Phone(ErrorMessage = "Invalid phone number.")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "New Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm New Password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
