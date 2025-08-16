using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class Rate
{
    public int BookId { get; set; }

    public int UserId { get; set; }

    public int Rate1 { get; set; }

    [ValidateNever]
    public virtual Book Book { get; set; } = null!;

    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
