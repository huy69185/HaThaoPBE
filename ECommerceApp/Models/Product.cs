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
