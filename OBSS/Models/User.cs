using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;

namespace OBSS.Models;

public partial class User
{
    public int UserId { get; set; }

    public int? UserType { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly Birthdate { get; set; }

    public int? GenderId { get; set; }

    public string? ContactNumber { get; set; }

    public string Email { get; set; } = null!;

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    [ValidateNever]
    public virtual Gender? Gender { get; set; } = null!;

    public virtual ICollection<Rate> Rates { get; set; } = new List<Rate>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    [ValidateNever]
    public virtual UserType? UserTypeNavigation { get; set; } = null!;
}
