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

    public virtual DbSet<Brand> Brands { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryAuditLog> CategoryAuditLogs { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }

    public virtual DbSet<LoginAudit> LoginAudits { get; set; }

    public virtual DbSet<Pharmacist> Pharmacists { get; set; }

    public virtual DbSet<Pharmacy> Pharmacies { get; set; }

    public virtual DbSet<PointSetting> PointSettings { get; set; }

    public virtual DbSet<PointsTransaction> PointsTransactions { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductBatch> ProductBatches { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<ProductInventory> ProductInventories { get; set; }

    public virtual DbSet<ProductTag> ProductTags { get; set; }

    public virtual DbSet<ProductUnit> ProductUnits { get; set; }

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

        modelBuilder.Entity<Brand>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.Name }, "UX_Brands_Pharmacy_Name")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.NameEn).HasMaxLength(150);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Brands)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Brands_Pharmacies");
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
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.ParentCategory).WithMany(p => p.InverseParentCategory)
                .HasForeignKey(d => d.ParentCategoryId)
                .HasConstraintName("FK_Categories_ParentCategory");
        });

        modelBuilder.Entity<CategoryAuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Category__3214EC07DD4B2CFE");

            entity.ToTable("CategoryAuditLog", "audit");

            entity.Property(e => e.ChangeDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ChangeType).HasMaxLength(50);
            entity.Property(e => e.ChangedBy).HasMaxLength(100);
            entity.Property(e => e.FieldName).HasMaxLength(100);
            entity.Property(e => e.Reason).HasMaxLength(500);

            entity.HasOne(d => d.Category).WithMany(p => p.CategoryAuditLogs)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CategoryAuditLog_Category");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC079B17B516");

            entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4D88A8935A").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CustomerType)
                .HasMaxLength(50)
                .HasDefaultValue("Regular");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Gender).HasMaxLength(10);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.NationalId).HasMaxLength(20);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.PointsExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Customers)
                .HasForeignKey(d => d.PharmacyId)
                .HasConstraintName("FK_Customers_Pharmacies");

            entity.HasOne(d => d.User).WithOne(p => p.Customer)
                .HasForeignKey<Customer>(d => d.UserId)
                .HasConstraintName("FK_Customers_Users");
        });

        modelBuilder.Entity<CustomerAddress>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07836F6163");

            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Region).HasMaxLength(100);
            entity.Property(e => e.Street).HasMaxLength(300);
            entity.Property(e => e.Title).HasMaxLength(100);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerAddresses)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerAddresses_Customer");
        });

        modelBuilder.Entity<LoginAudit>(entity =>
        {
            entity.HasIndex(e => e.CreatedAtUtc, "IX_LoginAudits_CreatedAtUtc");

            entity.HasIndex(e => new { e.IpAddress, e.CreatedAtUtc }, "IX_LoginAudits_Ip_CreatedAtUtc");

            entity.HasIndex(e => e.UserId, "IX_LoginAudits_UserId");

            entity.Property(e => e.CreatedAtUtc)
                .HasPrecision(3)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.FailureReason).HasMaxLength(50);
            entity.Property(e => e.IdentifierHash).HasMaxLength(32);
            entity.Property(e => e.IdentifierMasked).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.Outcome).HasMaxLength(20);
            entity.Property(e => e.TraceId).HasMaxLength(64);
            entity.Property(e => e.UserAgent).HasMaxLength(512);
        });

        modelBuilder.Entity<Pharmacist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC0781A1ADB4");

            entity.HasIndex(e => e.UserId, "UQ__Pharmaci__1788CC4DFC32560E").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Specialty).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Pharmacist)
                .HasForeignKey<Pharmacist>(d => d.UserId)
                .HasConstraintName("FK__Pharmacis__UserI__666B225D");
        });

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC07860E8703");

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

        modelBuilder.Entity<PointSetting>(entity =>
        {
            entity.HasKey(e => e.PharmacyId);

            entity.Property(e => e.PharmacyId).ValueGeneratedNever();
            entity.Property(e => e.EarnEnabled).HasDefaultValue(true);
            entity.Property(e => e.EarnPerAmount)
                .HasDefaultValue(10.00m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EarnPoints).HasDefaultValue(1);
            entity.Property(e => e.MaxRedeemPercent)
                .HasDefaultValue(50.00m)
                .HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PointValueEGP)
                .HasDefaultValue(0.10m)
                .HasColumnType("decimal(18, 4)");
            entity.Property(e => e.RedeemEnabled).HasDefaultValue(true);
            entity.Property(e => e.UpdatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Pharmacy).WithOne(p => p.PointSetting)
                .HasForeignKey<PointSetting>(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PointSettings_Pharmacies");
        });

        modelBuilder.Entity<PointsTransaction>(entity =>
        {
            entity.HasIndex(e => new { e.CustomerId, e.CreatedAt }, "IX_PointsTransactions_Customer_CreatedAt").IsDescending(false, true);

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ExpiresAt).HasPrecision(0);
            entity.Property(e => e.Note).HasMaxLength(300);

            entity.HasOne(d => d.Customer).WithMany(p => p.PointsTransactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PointsTransactions_Customers");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.PointsTransactions)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PointsTransactions_Pharmacies");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.CategoryId, e.IsActive }, "IX_Products_Pharmacy_Category_Active").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.Name }, "IX_Products_Pharmacy_Name").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.NameEn }, "IX_Products_Pharmacy_NameEn").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.NormalizedName, e.NormalizedNameEn }, "IX_Products_Pharmacy_Search").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.Barcode }, "UX_Products_Pharmacy_Barcode")
                .IsUnique()
                .HasFilter("([Barcode] IS NOT NULL AND [Barcode]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.InternationalCode }, "UX_Products_Pharmacy_InternationalCode")
                .IsUnique()
                .HasFilter("([InternationalCode] IS NOT NULL AND [InternationalCode]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.StockProductCode }, "UX_Products_Pharmacy_StockProductCode")
                .IsUnique()
                .HasFilter("([StockProductCode] IS NOT NULL AND [StockProductCode]<>'' AND [DeletedAt] IS NULL)");

            entity.Property(e => e.Barcode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.DosageForm).HasMaxLength(50);
            entity.Property(e => e.EarnPoints).HasDefaultValue(true);
            entity.Property(e => e.HasExpiry).HasDefaultValue(true);
            entity.Property(e => e.IntegratedAt).HasPrecision(0);
            entity.Property(e => e.InternationalCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsReturnable).HasDefaultValue(true);
            entity.Property(e => e.IsTaxable).HasDefaultValue(true);
            entity.Property(e => e.MinOrderQty).HasDefaultValue(1);
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.NameEn).HasMaxLength(250);
            entity.Property(e => e.NormalizedName).HasMaxLength(250);
            entity.Property(e => e.NormalizedNameEn).HasMaxLength(250);
            entity.Property(e => e.PackSize).HasMaxLength(80);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SearchKeywords).HasMaxLength(500);
            entity.Property(e => e.Slug).HasMaxLength(300);
            entity.Property(e => e.StockProductCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.StorageConditions).HasMaxLength(200);
            entity.Property(e => e.Strength).HasMaxLength(50);
            entity.Property(e => e.TaxCategoryCode).HasMaxLength(30);
            entity.Property(e => e.TrackInventory).HasDefaultValue(true);
            entity.Property(e => e.Unit).HasMaxLength(30);
            entity.Property(e => e.UpdatedAt).HasPrecision(0);
            entity.Property(e => e.UpdatedBy).HasMaxLength(100);
            entity.Property(e => e.VatRate).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Categories");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Products)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Products_Pharmacies");
        });

        modelBuilder.Entity<ProductBatch>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.StoreId, e.ExpirationDate }, "IX_ProductBatches_Product").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.ProductUnitId, e.ExpirationDate, e.Id }, "IX_ProductBatches_Unit_Expiry").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.StoreId, e.ProductUnitId, e.BatchNumber }, "UX_ProductBatches_Store_Unit_Batch")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

            entity.Property(e => e.BatchNumber).HasMaxLength(80);
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ReceivedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.ProductBatches)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBatches_Pharmacies");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductBatches)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBatches_Products");

            entity.HasOne(d => d.Store).WithMany(p => p.ProductBatches)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBatches_Stores");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.ProductBatches)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.ProductUnitId, d.ProductId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductBatches_ProductUnits_Composite");
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.ProductId }, "IX_ProductImages_Pharmacy_Product").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.ProductId, e.IsPrimary, e.SortOrder, e.Id }, "IX_ProductImages_Product_Sort")
                .IsDescending(false, true, false, false)
                .HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => e.ProductId, "UX_ProductImages_Product_Primary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1) AND [DeletedAt] IS NULL)");

            entity.Property(e => e.AltText).HasMaxLength(200);
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductImages_Pharmacies");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductImage)
                .HasForeignKey<ProductImage>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductImages_Products");
        });

        modelBuilder.Entity<ProductInventory>(entity =>
        {
            entity.HasKey(e => new { e.StoreId, e.ProductUnitId });

            entity.ToTable("ProductInventory");

            entity.HasIndex(e => new { e.PharmacyId, e.StoreId }, "IX_ProductInventory_Pharmacy_Store");

            entity.HasIndex(e => e.ProductId, "IX_ProductInventory_Product");

            entity.Property(e => e.LastStockUpdateAt).HasPrecision(0);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.ProductInventories)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInventory_Pharmacies");

            entity.HasOne(d => d.Product).WithMany(p => p.ProductInventories)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInventory_Products");

            entity.HasOne(d => d.Store).WithMany(p => p.ProductInventories)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInventory_Stores");

            entity.HasOne(d => d.ProductUnit).WithMany(p => p.ProductInventories)
                .HasPrincipalKey(p => new { p.Id, p.ProductId })
                .HasForeignKey(d => new { d.ProductUnitId, d.ProductId })
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductInventory_ProductUnits_Composite");
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
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductTags_Products");

            entity.HasOne(d => d.Tag).WithMany(p => p.ProductTags)
                .HasForeignKey(d => d.TagId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductTags_Tags");
        });

        modelBuilder.Entity<ProductUnit>(entity =>
        {
            entity.HasIndex(e => new { e.ProductId, e.IsActive, e.IsPrimary, e.SortOrder }, "IX_ProductUnits_Product_List").HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.Id, e.ProductId }, "UQ_ProductUnits_Id_ProductId").IsUnique();

            entity.HasIndex(e => new { e.PharmacyId, e.ProductId, e.UnitId }, "UX_ProductUnits_Pharmacy_Product_Unit")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.SKU }, "UX_ProductUnits_Pharmacy_SKU")
                .IsUnique()
                .HasFilter("([SKU] IS NOT NULL AND [SKU]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.UnitCode }, "UX_ProductUnits_Pharmacy_UnitCode")
                .IsUnique()
                .HasFilter("([UnitCode] IS NOT NULL AND [UnitCode]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => e.ProductId, "UX_ProductUnits_Product_Primary")
                .IsUnique()
                .HasFilter("([IsPrimary]=(1) AND [DeletedAt] IS NULL)");

            entity.Property(e => e.BaseQuantity).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.CostPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.CurrencyCode)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasDefaultValue("EGP")
                .IsFixedLength();
            entity.Property(e => e.DeletedAt).HasPrecision(0);
            entity.Property(e => e.DeletedBy).HasMaxLength(100);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.ListPrice).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PriceUpdatedAt).HasPrecision(0);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();
            entity.Property(e => e.SKU)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitCode)
                .HasMaxLength(50)
                .IsUnicode(false);
            entity.Property(e => e.UnitsPerParent).HasColumnType("decimal(18, 3)");
            entity.Property(e => e.UpdatedAt).HasPrecision(0);

            entity.HasOne(d => d.BaseUnit).WithMany(p => p.ProductUnitBaseUnits)
                .HasForeignKey(d => d.BaseUnitId)
                .HasConstraintName("FK_ProductUnits_BaseUnit");

            entity.HasOne(d => d.ParentProductUnit).WithMany(p => p.InverseParentProductUnit)
                .HasForeignKey(d => d.ParentProductUnitId)
                .HasConstraintName("FK_ProductUnits_ParentProductUnit");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.ProductUnits)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductUnits_Pharmacies");

            entity.HasOne(d => d.Product).WithOne(p => p.ProductUnit)
                .HasForeignKey<ProductUnit>(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductUnits_Products");

            entity.HasOne(d => d.Unit).WithMany(p => p.ProductUnitUnits)
                .HasForeignKey(d => d.UnitId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ProductUnits_Unit");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.IsActive, e.StartAt, e.EndAt }, "IX_Promotions_Pharmacy_Active_Dates");

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndAt).HasPrecision(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.StartAt).HasPrecision(0);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Promotions_Pharmacies");

            entity.HasMany(d => d.Categories).WithMany(p => p.Promotions)
                .UsingEntity<Dictionary<string, object>>(
                    "PromotionCategory",
                    r => r.HasOne<Category>().WithMany()
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PromotionCategories_Categories"),
                    l => l.HasOne<Promotion>().WithMany()
                        .HasForeignKey("PromotionId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_PromotionCategories_Promotions"),
                    j =>
                    {
                        j.HasKey("PromotionId", "CategoryId");
                        j.ToTable("PromotionCategories");
                        j.HasIndex(new[] { "CategoryId" }, "IX_PromotionCategories_CategoryId");
                    });
        });

        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.HasKey(e => new { e.PromotionId, e.ProductId });

            entity.HasIndex(e => e.ProductId, "IX_PromotionProducts_ProductId");

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionProducts_Products");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionProducts)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionProducts_Promotions");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasIndex(e => e.PharmacyId, "UX_Stores_OneDefaultPerPharmacy")
                .IsUnique()
                .HasFilter("([IsDefault]=(1) AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.Code }, "UX_Stores_Pharmacy_Code")
                .IsUnique()
                .HasFilter("([Code] IS NOT NULL AND [Code]<>'' AND [DeletedAt] IS NULL)");

            entity.HasIndex(e => new { e.PharmacyId, e.Name }, "UX_Stores_Pharmacy_Name")
                .IsUnique()
                .HasFilter("([DeletedAt] IS NULL)");

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
            entity.Property(e => e.Name).HasMaxLength(150);
            entity.Property(e => e.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(d => d.Pharmacy).WithOne(p => p.Store)
                .HasForeignKey<Store>(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Stores_Pharmacies");
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(e => new { e.PharmacyId, e.Name }, "UX_Tags_Pharmacy_Name").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasPrecision(0)
                .HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(80);
            entity.Property(e => e.NameEn).HasMaxLength(80);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Tags)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tags_Pharmacies");
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
