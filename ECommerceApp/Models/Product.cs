namespace ECommerceApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public double Rating { get; set; }
        public int Quantity { get; set; }
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
