using System.ComponentModel.DataAnnotations;
namespace ECommerceApp.Models
{
    public class VerifyEmailViewModel
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Code { get; set; }
        public bool IsExpired { get; set; }

        public DateTime TimeCreated { get; set; }
    }
}