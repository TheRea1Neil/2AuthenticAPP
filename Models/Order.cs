using System;
using System.Collections.Generic;

namespace _2AuthenticAPP.Models
{
    public partial class Order
    {
        public Order()
        {
            LineItems = new HashSet<LineItem>();
        }

        public int TransactionId { get; set; }
        public int? CustomerId { get; set; }
        public DateOnly? Date { get; set; }
        public string? DayOfWeek { get; set; }
        public byte? Time { get; set; }
        public string? EntryMode { get; set; }
        public double? Amount { get; set; }
        public string? TypeOfTransaction { get; set; }
        public string? CountryOfTransaction { get; set; }
        public string? ShippingAddress { get; set; }
        public string? Bank { get; set; }
        public string? TypeOfCard { get; set; }
        public int? Fraud { get; set; }

        // Add the LineItems navigation property
        public virtual ICollection<LineItem> LineItems { get; set; }
    }
}