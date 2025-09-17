using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class CartDetail
{
    public int CartId { get; set; }

    public int BookId { get; set; }

    public int Quantity { get; set; }

    public DateTime AddedDate { get; set; }

    [ValidateNever]
    public virtual Book Book { get; set; } = null!;

    [ValidateNever]
    public virtual Cart Cart { get; set; } = null!;
}
