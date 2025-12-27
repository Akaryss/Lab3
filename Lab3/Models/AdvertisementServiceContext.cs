using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Lab2.Models;

public partial class AdvertisementServiceContext : DbContext
{
    public AdvertisementServiceContext()
    {
    }

    public AdvertisementServiceContext(DbContextOptions<AdvertisementServiceContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActiveAdvertisement> ActiveAdvertisements { get; set; }

    public virtual DbSet<Advertisement> Advertisements { get; set; }

    public virtual DbSet<AdvertisementPhoto> AdvertisementPhotos { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Favorite> Favorites { get; set; }

    public virtual DbSet<Message> Messages { get; set; }

    public virtual DbSet<Region> Regions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserFavorite> UserFavorites { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    { }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActiveAdvertisement>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("ActiveAdvertisements");

            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.CityName).HasMaxLength(100);
            entity.Property(e => e.Price).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.PublicationDate).HasColumnType("datetime");
            entity.Property(e => e.RegionName).HasMaxLength(100);
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<Advertisement>(entity =>
        {
            entity.HasKey(e => e.AdvertisementId).HasName("PK__Advertis__C4C7F42DBA86F991");

            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.Price).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.PublicationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.RegionId).HasColumnName("RegionID");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Active");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Category).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Advertisements_Categories");

            entity.HasOne(d => d.Region).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.RegionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Advertisements_Regions");

            entity.HasOne(d => d.User).WithMany(p => p.Advertisements)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Advertisements_Users");
        });

        modelBuilder.Entity<AdvertisementPhoto>(entity =>
        {
            entity.HasKey(e => e.PhotoId).HasName("PK__Advertis__21B7B5824A7B2A30");

            entity.Property(e => e.PhotoId).HasColumnName("PhotoID");
            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.IsMain).HasDefaultValue(false);
            entity.Property(e => e.PhotoUrl)
                .HasMaxLength(500)
                .HasColumnName("PhotoURL");

            entity.HasOne(d => d.Advertisement).WithMany(p => p.AdvertisementPhotos)
                .HasForeignKey(d => d.AdvertisementId)
                .HasConstraintName("FK_AdvertisementPhotos_Advertisements");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A2B84DA3B19");

            entity.Property(e => e.CategoryId).HasColumnName("CategoryID");
            entity.Property(e => e.CategoryName).HasMaxLength(100);
            entity.Property(e => e.ParentCategoryId).HasColumnName("ParentCategoryID");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_Parent");
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasKey(e => e.FavoriteId).HasName("PK__Favorite__CE74FAF548F00C99");

            entity.Property(e => e.FavoriteId).HasColumnName("FavoriteID");
            entity.Property(e => e.AddedDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Advertisement).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.AdvertisementId)
                .HasConstraintName("FK_Favorites_Advertisements");

            entity.HasOne(d => d.User).WithMany(p => p.Favorites)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Favorites_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(e => e.MessageId).HasName("PK__Messages__C87C037CCD54E6A4");

            entity.Property(e => e.MessageId).HasColumnName("MessageID");
            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.MessageText).HasMaxLength(1000);
            entity.Property(e => e.ReceiverUserId).HasColumnName("ReceiverUserID");
            entity.Property(e => e.SendDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.SenderUserId).HasColumnName("SenderUserID");

            entity.HasOne(d => d.Advertisement).WithMany(p => p.Messages)
                .HasForeignKey(d => d.AdvertisementId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_Advertisements");

            entity.HasOne(d => d.ReceiverUser).WithMany(p => p.MessageReceiverUsers)
                .HasForeignKey(d => d.ReceiverUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_ReceiverUser");

            entity.HasOne(d => d.SenderUser).WithMany(p => p.MessageSenderUsers)
                .HasForeignKey(d => d.SenderUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Messages_SenderUser");
        });

        modelBuilder.Entity<Region>(entity =>
        {
            entity.HasKey(e => e.RegionId).HasName("PK__Regions__ACD8444322DC1710");

            entity.Property(e => e.RegionId).HasColumnName("RegionID");
            entity.Property(e => e.CityName).HasMaxLength(100);
            entity.Property(e => e.RegionName).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCACDF80A5C2");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E40AFE7773").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Users__A9D105341DC5E971").IsUnique();

            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Rating)
                .HasDefaultValueSql("((5.0))")
                .HasColumnType("decimal(3, 2)");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        modelBuilder.Entity<UserFavorite>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("UserFavorites");

            entity.Property(e => e.AddedDate).HasColumnType("datetime");
            entity.Property(e => e.AdvertisementId).HasColumnName("AdvertisementID");
            entity.Property(e => e.Price).HasColumnType("decimal(15, 2)");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.UserId).HasColumnName("UserID");
            entity.Property(e => e.Username).HasMaxLength(50);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
