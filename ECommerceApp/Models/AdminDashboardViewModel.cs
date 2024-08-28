using System;
using System.Collections.Generic;
using ECommerceApp.Controllers;
using Microsoft.AspNetCore.Identity;

namespace ECommerceApp.Models
{
    public class AdminDashboardViewModel
    {
        public List<Tuple<Order, string>> OrderTuples { get; set; }
        public List<IdentityUserWithClaims> Customers { get; set; }
        public List<IdentityUserWithClaims> Employees { get; set; }
        public Dictionary<string, UserMetadata> UserMetadatas { get; set; } // Thêm Dictionary để chứa thông tin từ UserMetadata
        public IEnumerable<dynamic> RevenueData { get; set; }
        public IEnumerable<dynamic> RegistrationData { get; set; }
    }
}
