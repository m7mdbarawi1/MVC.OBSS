using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class vw_BookRating
{
    public int BookId { get; set; }

    public decimal? AverageRate { get; set; }

    public int? RatingsCount { get; set; }
}
