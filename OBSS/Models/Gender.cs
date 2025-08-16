using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class Gender
{
    public int GenderId { get; set; }

    public string GenderDesc { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
