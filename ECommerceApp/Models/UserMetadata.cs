using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace ECommerceApp.Models
{
    public class UserMetadata
    {
        [Key]
        [ForeignKey("IdentityUser")]
        public string Id { get; set; } // Liên kết với IdentityUser qua UserId
        public DateTime RegisterDate { get; set; } // Ngày tạo tài khoản

        public virtual IdentityUser IdentityUser { get; set; } // Liên kết với IdentityUser
    }
}
