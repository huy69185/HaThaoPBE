namespace ECommerceApp.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public string TransactionId { get; set; }
        public string Bank { get; set; }
        public DateTime TransactionDate { get; set; }
        public Order Order { get; set; }
    }
}
