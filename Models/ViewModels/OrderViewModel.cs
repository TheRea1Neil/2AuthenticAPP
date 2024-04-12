// Models/ViewModels/OrderViewModel.cs
namespace _2AuthenticAPP.Models.ViewModels
{
    public class OrderViewModel
    {
        public List<FraudPrediction> FraudPredictions { get; set; } = new List<FraudPrediction>();
        public string SearchString { get; set; }
        public bool ShowFraudOnly { get; set; }
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
    }
}