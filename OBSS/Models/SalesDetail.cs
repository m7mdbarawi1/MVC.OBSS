using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class SalesDetail
{
    public int DetailId { get; set; }

    public int SaleId { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    [ValidateNever]
    public virtual Book Book { get; set; } = null!;

    [ValidateNever]
    public virtual Sale Sale { get; set; } = null!;
}
