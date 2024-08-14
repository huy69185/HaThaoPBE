using ECommerceApp.Models;
namespace ECommerceApp.Models
{
    public class CheckoutViewModel
    {
        public List<CartItem> CartItems { get; set; }
        public string Address { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string PaymentMethod { get; set; }
    }
}