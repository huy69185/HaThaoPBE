using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace ECommerceApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string ImageUrl { get; set; }
        public string Category { get; set; }
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
        public double Rating { get; set; }  // Thêm thuộc tính Rating
        public int Quantity { get; set; }

        // New fields added
        public string Brand { get; set; }         // Thương hiệu
        public string ManufacturedIn { get; set; } // Sản xuất tại
        public string Fragrance { get; set; }     // Mùi hương
        public string Usage { get; set; }         // Sử dụng cho
        public string Weight { get; set; }        // Khối lượng
        public string Ingredients { get; set; }   // Thành phần chính
        public string Storage { get; set; }       // Bảo quản

        [NotMapped]
        public double AverageRating
        {
            get
            {
                if (Votes != null && Votes.Any())
                {
                    return Votes.Average(v => v.Rating);
                }
                return 0;
            }
        }
    }
}
