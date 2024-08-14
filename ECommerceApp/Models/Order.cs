﻿using ECommerceApp.Models;
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
        public List<OrderItem> OrderItems { get; set; }
    }
}