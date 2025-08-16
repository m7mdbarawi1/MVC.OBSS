using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using OBSS.Models;

namespace OBSS.Data;

public partial class OBSSContext : DbContext
{
    public OBSSContext(DbContextOptions<OBSSContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; }

    public virtual DbSet<Cart> Carts { get; set; }

    public virtual DbSet<CartDetail> CartDetails { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Gender> Genders { get; set; }

    public virtual DbSet<Rate> Rates { get; set; }

    public virtual DbSet<Sale> Sales { get; set; }

    public virtual DbSet<SalesDetail> SalesDetails { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserType> UserTypes { get; set; }

    public virtual DbSet<vw_BookRating> vw_BookRatings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(e => e.BookId).HasName("PK__Books__3DE0C207A3C389A9");

            entity.HasIndex(e => e.Author, "IX_Books_Author");

            entity.HasIndex(e => e.CategoryId, "IX_Books_CategoryId");

            entity.HasIndex(e => e.Subject, "IX_Books_Subject");

            entity.HasIndex(e => e.BookTitle, "IX_Books_Title");

            entity.HasIndex(e => new { e.BookTitle, e.Author, e.PublishingHouse }, "UQ_Books").IsUnique();

            entity.Property(e => e.Author)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.BookTitle)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CoverImageUrl)
                .HasMaxLength(250)
                .IsUnicode(false);
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .IsUnicode(false);
            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PublishingHouse)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Subject)
                .HasMaxLength(250)
                .IsUnicode(false);

            entity.HasOne(d => d.Category).WithMany(p => p.Books)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Books__CategoryI__5812160E");
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.CartId).HasName("PK__Cart__51BCD7B7D7B5FC01");

            entity.ToTable("Cart");

            entity.HasOne(d => d.User).WithMany(p => p.Carts)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Cart__UserId__619B8048");
        });

        modelBuilder.Entity<CartDetail>(entity =>
        {
            entity.HasKey(e => new { e.CartId, e.BookId });

            entity.HasOne(d => d.Book).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__CartDetai__BookI__656C112C");

            entity.HasOne(d => d.Cart).WithMany(p => p.CartDetails)
                .HasForeignKey(d => d.CartId)
                .HasConstraintName("FK__CartDetai__CartI__6477ECF3");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0BC79E268A");

            entity.HasIndex(e => e.CategoryDesc, "UQ__Categori__9D2327C6134478E5").IsUnique();

            entity.Property(e => e.CategoryDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Gender>(entity =>
        {
            entity.HasKey(e => e.GenderId).HasName("PK__Genders__4E24E9F7FD79FB19");

            entity.Property(e => e.GenderId).ValueGeneratedNever();
            entity.Property(e => e.GenderDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Rate>(entity =>
        {
            entity.HasKey(e => new { e.BookId, e.UserId });

            entity.ToTable("Rate");

            entity.Property(e => e.Rate1).HasColumnName("Rate");

            entity.HasOne(d => d.Book).WithMany(p => p.Rates)
                .HasForeignKey(d => d.BookId)
                .HasConstraintName("FK__Rate__BookId__5CD6CB2B");

            entity.HasOne(d => d.User).WithMany(p => p.Rates)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Rate__UserId__5DCAEF64");
        });

        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.SaleId).HasName("PK__Sales__1EE3C3FF18E58646");

            entity.HasOne(d => d.User).WithMany(p => p.Sales)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Sales__UserId__693CA210");
        });

        modelBuilder.Entity<SalesDetail>(entity =>
        {
            entity.HasKey(e => new { e.SaleId, e.DetailId });

            entity.Property(e => e.Price).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.Book).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.BookId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__SalesDeta__BookI__6D0D32F4");

            entity.HasOne(d => d.Sale).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.SaleId)
                .HasConstraintName("FK__SalesDeta__SaleI__6C190EBB");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4C79AD15C7");

            entity.HasIndex(e => e.Email, "UQ__Users__A9D1053417EDEF4C").IsUnique();

            entity.HasIndex(e => e.UserName, "UQ__Users__C9F284561B947A5D").IsUnique();

            entity.Property(e => e.ContactNumber)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Email)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.Password)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.UserName)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.Gender).WithMany(p => p.Users)
                .HasForeignKey(d => d.GenderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__GenderId__5165187F");

            entity.HasOne(d => d.UserTypeNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.UserType)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Users__UserType__5070F446");
        });

        modelBuilder.Entity<UserType>(entity =>
        {
            entity.HasKey(e => e.TypeId).HasName("PK__UserType__516F03B5617729E9");

            entity.Property(e => e.TypeId).ValueGeneratedNever();
            entity.Property(e => e.TypeDesc)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<vw_BookRating>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_BookRatings");

            entity.Property(e => e.AverageRate).HasColumnType("decimal(38, 6)");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
