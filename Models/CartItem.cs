using System;
using System.Collections.Generic;

namespace _2AuthenticAPP.Models;

public partial class CartItem
{
    public int CustomerId { get; set; }

    public int ProductId { get; set; }

    public int? Qty { get; set; }
}
