using System;
using System.Collections.Generic;
using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public partial class RepositoryContext : DbContext
{
    public RepositoryContext()
    {
    }

    public RepositoryContext(DbContextOptions<RepositoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AspNetRole> AspNetRoles { get; set; }

    public virtual DbSet<AspNetRoleClaim> AspNetRoleClaims { get; set; }

    public virtual DbSet<AspNetUser> AspNetUsers { get; set; }

    public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }

    public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }

    public virtual DbSet<AspNetUserToken> AspNetUserTokens { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Company> Companies { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Pharmacy> Pharmacies { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductTag> ProductTags { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionProduct> PromotionProducts { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<Unit> Units { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=pharma_shope_db;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_100_CI_AS_KS_WS_SC_UTF8");

        modelBuilder.Entity<AspNetRole>(entity =>
        {
            entity.HasIndex(e => e.NormalizedName, "RoleNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedName] IS NOT NULL)");

            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Name).HasMaxLength(256);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.NormalizedName).HasMaxLength(256);
        });

        modelBuilder.Entity<AspNetRoleClaim>(entity =>
        {
            entity.HasIndex(e => e.RoleId, "IX_AspNetRoleClaims_RoleId");

            entity.HasOne(d => d.Role).WithMany(p => p.AspNetRoleClaims).HasForeignKey(d => d.RoleId);
        });

        modelBuilder.Entity<AspNetUser>(entity =>
        {
            entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");

            entity.HasIndex(e => e.PhoneNumber, "PhoneNumberIndex")
                .IsUnique()
                .HasFilter("([PhoneNumber] IS NOT NULL)");

            entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                .IsUnique()
                .HasFilter("([NormalizedUserName] IS NOT NULL)");

            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
            entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(50);
            entity.Property(e => e.UserName).HasMaxLength(256);

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "AspNetUserRole",
                    r => r.HasOne<AspNetRole>().WithMany().HasForeignKey("RoleId"),
                    l => l.HasOne<AspNetUser>().WithMany().HasForeignKey("UserId"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("AspNetUserRoles");
                        j.HasIndex(new[] { "RoleId" }, "IX_AspNetUserRoles_RoleId");
                    });
        });

        modelBuilder.Entity<AspNetUserClaim>(entity =>
        {
            entity.HasIndex(e => e.UserId, "IX_AspNetUserClaims_UserId");

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserClaims).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserLogin>(entity =>
        {
            entity.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            entity.HasIndex(e => e.UserId, "IX_AspNetUserLogins_UserId");

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.ProviderKey).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserLogins).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<AspNetUserToken>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });

            entity.Property(e => e.LoginProvider).HasMaxLength(128);
            entity.Property(e => e.Name).HasMaxLength(128);

            entity.HasOne(d => d.User).WithMany(p => p.AspNetUserTokens).HasForeignKey(d => d.UserId);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(e => e.ImageId, "IX_Categories_ImageId").HasFilter("([ImageId] IS NOT NULL)");

            entity.HasIndex(e => e.ParentCategoryId, "IX_Categories_ParentCategoryId");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageId).HasMaxLength(128);
            entity.Property(e => e.ImageUpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NameAr).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_ParentCategory");
        });

        modelBuilder.Entity<Company>(entity =>
        {
            entity.HasIndex(e => e.IsActive, "IX_Companies_IsActive").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.NameEn, "IX_Companies_NameEn").HasFilter("([DeletedAt] IS NULL AND [NameEn] IS NOT NULL)");

            entity.HasIndex(e => e.NameAr, "UX_Companies_NameAr")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NameAr).HasMaxLength(150);
            entity.Property(e => e.NameEn).HasMaxLength(150);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Customers_Email").HasFilter("([Email] IS NOT NULL)");

            entity.HasIndex(e => e.PhoneNumber, "IX_Customers_PhoneNumber").HasFilter("([PhoneNumber] IS NOT NULL)");

            entity.HasIndex(e => e.UserId, "UX_Customers_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CustomerType)
                .HasMaxLength(50)
                .HasDefaultValue("Regular");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullNameAr).HasMaxLength(200);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PointsExpiryDate).HasPrecision(0);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.User).WithOne(p => p.Customer).HasForeignKey<Customer>(d => d.UserId);
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasIndex(e => e.CustomerId, "IX_CustomerAddresses_CustomerId");

            entity.HasIndex(e => new { e.CustomerId, e.IsDefault }, "IX_CustomerAddresses_CustomerId_IsDefault");

            entity.HasIndex(e => e.CustomerId, "UX_CustomerAddresses_CustomerId_Default")
                .IsUnique()
                .HasFilter("([IsDefault]=(1))");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Street).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Customer).WithOne(p => p.CustomerAddress)
                .HasForeignKey<CustomerAddress>(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasIndex(e => e.Email, "IX_Employees_Email").HasFilter("([Email] IS NOT NULL AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => e.EmployeeCode, "IX_Employees_EmployeeCode").HasFilter("([EmployeeCode] IS NOT NULL AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => e.PharmacyId, "IX_Employees_PharmacyId").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.PhoneNumber, "IX_Employees_PhoneNumber").HasFilter("([PhoneNumber] IS NOT NULL AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => e.UserId, "UX_Employees_UserId")
                .IsUnique()
                .HasFilter("([UserId] IS NOT NULL AND [DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.EmployeeCode).HasMaxLength(50);
            entity.Property(e => e.FullNameAr).HasMaxLength(200);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.JobTitle).HasMaxLength(100);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.User).WithOne(p => p.Employee).HasForeignKey<Employee>(d => d.UserId);
        });

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC075D621057");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.LicenseNumber).HasMaxLength(100);
            entity.Property(e => e.Longitude).HasColumnType("decimal(10, 7)");
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.OwnerName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => new { e.StoreId, e.CategoryId, e.IsActive, e.IsAvailable }, "IX_Products_List").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.StoreId, e.NameAr }, "IX_Products_NameAr").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.StoreId, e.NameEn }, "IX_Products_NameEn").HasFilter("([DeletedAt] IS NULL AND [NameEn] IS NOT NULL)");

            entity.HasIndex(e => new { e.StoreId, e.HasPromotion, e.PromotionEndsAt }, "IX_Products_Promotions").HasFilter("([DeletedAt] IS NULL AND [HasPromotion]=(1))");

            entity.HasIndex(e => new { e.StoreId, e.ErpProductId }, "IX_Products_Store_ErpProductId").HasFilter("([DeletedAt] IS NULL AND [ErpProductId] IS NOT NULL)");

            entity.HasIndex(e => new { e.StoreId, e.InternationalCode }, "UX_Products_Store_InternationalCode")
                .IsUnique()
                .HasFilter("([InternationalCode] IS NOT NULL AND [InternationalCode]<>'' AND [DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.ErpProductId).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.HasExpiry).HasDefaultValue(true);
            entity.Property(e => e.InnerUnitPrice)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.IntegratedAt).HasPrecision(0);
            entity.Property(e => e.InternationalCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsReturnable).HasDefaultValue(true);
            entity.Property(e => e.LastStockSyncAt).HasPrecision(0);
            entity.Property(e => e.MinOrderQty).HasDefaultValue(1);
            entity.Property(e => e.NameAr).HasMaxLength(250);
            entity.Property(e => e.NameEn).HasMaxLength(250);
            entity.Property(e => e.OuterUnitPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromotionDiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PromotionEndsAt).HasPrecision(0);
            entity.Property(e => e.PromotionStartsAt).HasPrecision(0);
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.SearchKeywords).HasMaxLength(500);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Company).WithMany(p => p.Products)
                .HasForeignKey(d => d.CompanyId)
                .HasConstraintName("FK_Products_Companies");

            entity.HasOne(d => d.InnerUnit).WithMany(p => p.ProductInnerUnits)
                .HasForeignKey(d => d.InnerUnitId)
                .HasConstraintName("FK_Products_Units_Inner");

            entity.HasOne(d => d.OuterUnit).WithMany(p => p.ProductOuterUnits)
                .HasForeignKey(d => d.OuterUnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Units_Outer");

            entity.HasOne(d => d.Store).WithMany(p => p.Products)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Stores");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.ProductId }, "IX_ProductImages_PharmacyId_ProductId").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.ProductId, "IX_ProductImages_ProductId").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.ProductId, e.IsPrimary, e.SortOrder, e.Id }, "IX_ProductImages_ProductId_Sort")
                .IsDescending(false, true, false, false)
                .HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.ProductId, e.IsPrimary }, "UX_ProductImages_ProductId_Primary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1) AND [DeletedAt] IS NULL)");

            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(600);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.ThumbnailUrl).HasMaxLength(600);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<ProductTag>(entity =>
        {
            entity.HasKey(e => new { e.ProductId, e.TagId });

            entity.HasIndex(e => e.TagId, "IX_ProductTags_TagId");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductTags)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(d => d.Tag).WithMany(p => p.ProductTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasIndex(e => new { e.IsActive, e.StartAt, e.EndAt }, "IX_Promotions_ActiveWindow").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.ErpPgoId, "UX_Promotions_ErpPgoId")
                .IsUnique()
                .HasFilter("([ErpPgoId] IS NOT NULL AND [DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DiscountPercent).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.EndAt).HasPrecision(0);
            entity.Property(e => e.ErpPgoId).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.Notes).HasMaxLength(250);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.StartAt).HasPrecision(0);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
        });

        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.HasIndex(e => e.ErpProductId, "IX_PromotionProducts_ErpProductId").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.ProductId, "IX_PromotionProducts_ProductId").HasFilter("([DeletedAt] IS NULL AND [ProductId] IS NOT NULL)");

            entity.HasIndex(e => e.PromotionId, "IX_PromotionProducts_PromotionId").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PromotionId, e.ErpProductId }, "UX_PromotionProducts_PromotionId_ErpProductId")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.ErpOfferId).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ErpProductId).HasColumnType("decimal(18, 0)");

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionProducts).HasForeignKey(d => d.ProductId);

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionProducts).HasForeignKey(d => d.PromotionId);
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.IsActive }, "IX_Stores_PharmacyId_IsActive").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.NameAr }, "IX_Stores_PharmacyId_NameAr").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.NameEn }, "IX_Stores_PharmacyId_NameEn").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.Code }, "UX_Stores_PharmacyId_Code")
                .IsUnique()
                .HasFilter("([Code] IS NOT NULL AND [Code]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => e.PharmacyId, "UX_Stores_PharmacyId_Default")
                .IsUnique()
                .HasFilter("([IsDefault]=(1) AND [DeletedAt] IS NULL)");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NameAr).HasMaxLength(150);
            entity.Property(e => e.NameEn).HasMaxLength(150);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Pharmacy).WithOne(p => p.Store)
                .HasForeignKey<Store>(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.Name }, "UX_Tags_PharmacyId_Name").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.NameEn).HasMaxLength(80);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Tags)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull);
        });

        modelBuilder.Entity<Unit>(entity =>
        {
            entity.HasIndex(e => e.Code, "UX_Units_Code").IsUnique();

            entity.HasIndex(e => e.NameEn, "UX_Units_NameEn").IsUnique();

            entity.Property(e => e.Code)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NameAr).HasMaxLength(60);
            entity.Property(e => e.NameEn).HasMaxLength(60);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
