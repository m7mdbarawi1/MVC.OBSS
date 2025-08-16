using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class Sale
{
    public int SaleId { get; set; }

    public int UserId { get; set; }

    public DateOnly SaleDate { get; set; }

    public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();
    
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
