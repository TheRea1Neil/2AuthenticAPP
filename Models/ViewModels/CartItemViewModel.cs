namespace _2AuthenticAPP.Models.ViewModels
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal? Price { get; set; }
        public string ImgLink { get; set; }
        public string Description { get; set; }
        public int? Year { get; set; }
        public int? NumParts { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
        public string Category { get; set; }
        public int Qty { get; set; } // Add the Qty property
    }
}
