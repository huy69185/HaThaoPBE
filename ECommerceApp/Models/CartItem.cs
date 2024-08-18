namespace ECommerceApp.Models
{
    public class CartItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public bool SelectedItem { get; set; }  // Thuộc tính mới để xác định sản phẩm đã được chọn hay chưa
    }
}
