using System;
using System.Collections.Generic;

namespace _2AuthenticAPP.Models;

public partial class LineItem
{
    public int? TransactionId { get; set; }

    public int? ProductId { get; set; }

    public int? Qty { get; set; }

    public byte? Rating { get; set; }
}
