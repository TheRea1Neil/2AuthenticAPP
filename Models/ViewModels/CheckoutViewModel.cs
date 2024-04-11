namespace _2AuthenticAPP.Models.ViewModels
{
    public class CheckoutViewModel
    {
        public IEnumerable<CartItemViewModel> CartItems { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
