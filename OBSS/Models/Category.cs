using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryDesc { get; set; } = null!;

    public virtual ICollection<Book> Books { get; set; } = new List<Book>();
}
