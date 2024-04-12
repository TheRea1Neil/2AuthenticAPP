using System;
using System.Collections.Generic;

namespace _2AuthenticAPP.Models;

public partial class LineItem
{
    public int TransactionId { get; set; }

    public int ProductId { get; set; }

    public int? Qty { get; set; }
        public byte? Rating { get; set; }

        // Add the navigation properties
        public virtual Order Order { get; set; }
        public virtual Product Product { get; set; }
    }

