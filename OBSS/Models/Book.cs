using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // ✅ Add this

namespace OBSS.Models
{
    public partial class Book
    {
        public int BookId { get; set; }

        public int CategoryId { get; set; }

        public string Subject { get; set; } = null!;

        public string BookTitle { get; set; } = null!;

        public string Author { get; set; } = null!;

        public DateOnly? PublishDate { get; set; }

        public string? PublishingHouse { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity cannot be negative")] // ✅ Added
        public int QuantityInStore { get; set; }

        public string? CoverImageUrl { get; set; }

        public string Description { get; set; } = null!;

        [Range(0, double.MaxValue, ErrorMessage = "Price cannot be negative")] // ✅ Added
        public decimal Price { get; set; }

        public virtual ICollection<CartDetail> CartDetails { get; set; } = new List<CartDetail>();

        [ValidateNever]
        public virtual Category Category { get; set; } = null!;

        public virtual ICollection<Rate> Rates { get; set; } = new List<Rate>();

        public virtual ICollection<SalesDetail> SalesDetails { get; set; } = new List<SalesDetail>();
    }
}