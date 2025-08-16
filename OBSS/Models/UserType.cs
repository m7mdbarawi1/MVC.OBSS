using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class UserType
{
    public int TypeId { get; set; }

    public string TypeDesc { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
