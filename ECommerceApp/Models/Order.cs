﻿using Microsoft.AspNetCore.Identity;

namespace ECommerceApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public string Address { get; set; }
        public string RecipientName { get; set; }
        public string RecipientPhone { get; set; }
        public string PaymentMethod { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; } // Thêm trường PaymentStatus
        public DateTime OrderDate { get; set; }
        public List<OrderItem> OrderItems { get; set; }
        public List<Transaction> Transactions { get; set; }
        public string UserId { get; set; }
        public IdentityUser User { get; set; }
    }
}
