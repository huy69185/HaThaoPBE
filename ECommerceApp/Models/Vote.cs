namespace ECommerceApp.Models
{
    public class Vote
    {
        public int Id { get; set; }
        public int ProductID { get; set; }
        public string Comment { get; set; }
        public double Rating { get; set; }
        public string CustomerID { get; set; }
        public string CustomerName { get; set; }
        public Product Product { get; set; }
    }
}
