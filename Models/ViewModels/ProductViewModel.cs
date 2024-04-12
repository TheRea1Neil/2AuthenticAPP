using System.ComponentModel.DataAnnotations;

namespace _2AuthenticAPP.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        [DisplayFormat(DataFormatString = "{0:C}")]
        public decimal? Price { get; set; }
        public string ImgLink { get; set; }
        public double? AverageRating { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public int? NumParts { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string Category { get; set; }
        public List<Product>? Recommendations { get; set; } = new List<Product>();
    }
}
