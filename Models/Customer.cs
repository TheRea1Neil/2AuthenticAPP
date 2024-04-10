using System;
using System.Collections.Generic;

namespace _2AuthenticAPP.Models;

public partial class Customer
{
    public long CustomerId { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? BirthDate { get; set; }

    public string? CountryOfResidence { get; set; }

    public string? Gender { get; set; }

    public double? Age { get; set; }

    public string? Email { get; set; }
}
