using Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Repository;

public partial class RepositoryContext : DbContext
{


    public RepositoryContext(DbContextOptions<RepositoryContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<CategoryAuditLog> CategoryAuditLogs { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerAddress> CustomerAddresses { get; set; }

    public virtual DbSet<CustomerPointsHistory> CustomerPointsHistories { get; set; }

    public virtual DbSet<Pharmacist> Pharmacists { get; set; }

    public virtual DbSet<Pharmacy> Pharmacies { get; set; }

    public virtual DbSet<Prescription> Prescriptions { get; set; }

    public virtual DbSet<PrescriptionImage> PrescriptionImages { get; set; }

    public virtual DbSet<PrescriptionItem> PrescriptionItems { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<ProductImage> ProductImages { get; set; }

    public virtual DbSet<PromoCode> PromoCodes { get; set; }

    public virtual DbSet<PromoCodeUsage> PromoCodeUsages { get; set; }

    public virtual DbSet<Promotion> Promotions { get; set; }

    public virtual DbSet<PromotionCategory> PromotionCategories { get; set; }

    public virtual DbSet<PromotionProduct> PromotionProducts { get; set; }

    public virtual DbSet<PromotionUsage> PromotionUsages { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SalesDetail> SalesDetails { get; set; }

    public virtual DbSet<SalesDetailsReturn> SalesDetailsReturns { get; set; }

    public virtual DbSet<SalesHeader> SalesHeaders { get; set; }

    public virtual DbSet<SalesHeaderReturn> SalesHeaderReturns { get; set; }



    public virtual DbSet<Tag> Tags { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
//    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//        => optionsBuilder.UseSqlServer("Data Source=.;Initial Catalog=pharma_shope_db; Trusted_Connection=True; TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("Arabic_100_CI_AS_KS_WS_SC_UTF8");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Admins__3214EC07E1F84886");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.FullName).HasMaxLength(200);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Role).HasMaxLength(100);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("FK_Categories_ParentCategory");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageId).HasMaxLength(128);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Products");

            entity.Property(e => e.Barcode)
                .HasMaxLength(50)
                .IsUnicode(false)
                .IsRequired();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .IsRequired();

            entity.Property(e => e.CreatedBy).HasMaxLength(100);

            entity.Property(e => e.IntegratedAt).HasColumnType("datetime");

            entity.Property(e => e.InternationalCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.StockProductCode)
                .HasMaxLength(50)
                .IsUnicode(false);

            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .IsRequired();

            entity.Property(e => e.OldPrice).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.Points).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PromoDisc).HasColumnType("decimal(18, 2)");

            entity.Property(e => e.PromoEndDate).HasColumnType("datetime");

            entity.Property(e => e.Name)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(e => e.NameEn)
                .HasMaxLength(250)
                .IsRequired();

            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsAvailable).HasDefaultValue(true);
            entity.Property(e => e.IsIntegrated).HasDefaultValue(false);
            entity.Property(e => e.IsGroupOffer).HasDefaultValue(false);

            // Unique Constraints
            entity.HasIndex(e => e.Barcode)
                .IsUnique()
                .HasDatabaseName("UQ_Products_Barcode");

            entity.HasIndex(e => e.InternationalCode)
                .IsUnique()
                .HasDatabaseName("UQ_Products_InternationalCode");

            entity.HasIndex(e => e.StockProductCode)
                .IsUnique()
                .HasDatabaseName("UQ_Products_StockProductCode");

            // Indexes
            entity.HasIndex(e => e.CategoryId)
                .HasDatabaseName("IX_Products_CategoryId");

            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_Products_CreatedAt");

            entity.HasIndex(e => e.Name)
                .HasDatabaseName("IX_Products_Name");

            entity.HasIndex(e => e.NameEn)
                .HasDatabaseName("IX_Products_NameEn");

            // Relations
            entity.HasOne(d => d.Category)
                .WithMany(p => p.Products)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.Restrict) // لأن CategoryId NOT NULL
                .HasConstraintName("FK_Products_Category_CategoryId");

            // Many-to-Many (Products <-> Tags)
            entity.HasMany(d => d.Tags).WithMany(p => p.Products)
                .UsingEntity<Dictionary<string, object>>(
                    "ProductTag",
                    r => r.HasOne<Tag>().WithMany()
                        .HasForeignKey("TagId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProductTags_TagId"),
                    l => l.HasOne<Product>().WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_ProductTags_ProductId"),
                    j =>
                    {
                        j.HasKey("ProductId", "TagId").HasName("PK_ProductTags");
                        j.ToTable("ProductTags");
                    });
        });

        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__ProductI__3214EC07EA62E4AF");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ImageUrl).HasMaxLength(500);

            entity.HasOne(d => d.Product).WithMany(p => p.ProductImages)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_ProductImages_Products_ID");
        });


        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07E193979F");

            entity.HasIndex(e => e.UserId, "UQ__Customer__1788CC4D72C79C00").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07123F6CF8");

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

        modelBuilder.Entity<CustomerPointsHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Customer__3214EC07B604E456");

            entity.ToTable("CustomerPointsHistory");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.ExpiryDate).HasColumnType("datetime");
            entity.Property(e => e.Reason).HasMaxLength(200);
            entity.Property(e => e.SourceType).HasMaxLength(50);

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerPointsHistories)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerPointsHistory_Customers");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.CustomerPointsHistories)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_CustomerPointsHistory_Pharmacies");
        });

        modelBuilder.Entity<Pharmacist>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC07403F89A5");

            entity.HasIndex(e => e.UserId, "UQ__Pharmaci__1788CC4D4F98E1E7").IsUnique();

            entity.Property(e => e.FullName).HasMaxLength(100);
            entity.Property(e => e.FullNameEn).HasMaxLength(200);
            entity.Property(e => e.Specialty).HasMaxLength(100);

            entity.HasOne(d => d.User).WithOne(p => p.Pharmacist)
                .HasForeignKey<Pharmacist>(d => d.UserId)
                .HasConstraintName("FK__Pharmacis__UserI__59063A47");
        });

        modelBuilder.Entity<Pharmacy>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Pharmaci__3214EC072A9CDB39");

            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.LicenseNumber).HasMaxLength(100);
            entity.Property(e => e.Name).HasMaxLength(200);
            entity.Property(e => e.NameEn).HasMaxLength(200);
            entity.Property(e => e.OwnerName).HasMaxLength(150);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
        });

        modelBuilder.Entity<Prescription>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3214EC073DC67469");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReviewedAt).HasColumnType("datetime");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");

            entity.HasOne(d => d.Customer).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Custo__7E37BEF6");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Pharm__7F2BE32F");

            entity.HasOne(d => d.ReviewedByUser).WithMany(p => p.Prescriptions)
                .HasForeignKey(d => d.ReviewedByUserId)
                .HasConstraintName("FK__Prescript__Revie__00200768");
        });

        modelBuilder.Entity<PrescriptionImage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3214EC07796AEE98");

            entity.Property(e => e.FileUrl).HasMaxLength(500);
            entity.Property(e => e.UploadedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionImages)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Presc__03F0984C");
        });

        modelBuilder.Entity<PrescriptionItem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Prescrip__3214EC076D19F20D");

            entity.HasOne(d => d.Prescription).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.PrescriptionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Presc__06CD04F7");

            entity.HasOne(d => d.Product).WithMany(p => p.PrescriptionItems)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Prescript__Produ__07C12930");
        });

        //=============

       

        modelBuilder.Entity<PromoCode>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PromoCod__3214EC07929DFAC2");

            entity.HasIndex(e => e.Code, "UQ__PromoCod__A25C5AA74575CD7A").IsUnique();

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DescriptionEn).HasMaxLength(500);
            entity.Property(e => e.DiscountType).HasMaxLength(20);
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PromoType).HasMaxLength(50);
            entity.Property(e => e.StartDate).HasColumnType("datetime");

            entity.HasOne(d => d.Category).WithMany(p => p.PromoCodes)
                .HasForeignKey(d => d.CategoryId)
                .HasConstraintName("FK_PromoCodes_Categories");

            entity.HasOne(d => d.Customer).WithMany(p => p.PromoCodes)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_PromoCodes_Customers");

            entity.HasOne(d => d.Product).WithMany(p => p.PromoCodes)
                .HasForeignKey(d => d.ProductId)
                .HasConstraintName("FK_PromoCodes_Products");
        });

        modelBuilder.Entity<PromoCodeUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__PromoCod__3214EC0782F916B8");

            entity.Property(e => e.UsedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Customer).WithMany(p => p.PromoCodeUsages)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromoCodeUsages_Customers");

            entity.HasOne(d => d.PromoCode).WithMany(p => p.PromoCodeUsages)
                .HasForeignKey(d => d.PromoCodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromoCodeUsages_PromoCodes");
        });

        modelBuilder.Entity<Promotion>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC0720743A62");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DiscountValue).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.EndsAt).HasColumnType("datetime");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MinPurchaseAmount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.PromoType).HasMaxLength(50);
            entity.Property(e => e.StartsAt).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(200);
            entity.Property(e => e.TitleEn).HasMaxLength(200);

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.Promotions)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Promotions_Pharmacies");
        });

        modelBuilder.Entity<PromotionCategory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC0750CE77C0");

            entity.HasOne(d => d.Category).WithMany(p => p.PromotionCategories)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionCategories_Categories");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionCategories)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionCategories_Promotions");
        });

        modelBuilder.Entity<PromotionProduct>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC073D731E3C");

            entity.HasOne(d => d.Product).WithMany(p => p.PromotionProducts)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionProducts_Products");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionProducts)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionProducts_Promotions");
        });

        modelBuilder.Entity<PromotionUsage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Promotio__3214EC07041EE0CC");

            entity.ToTable("PromotionUsage");

            entity.Property(e => e.UsageDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Promotion).WithMany(p => p.PromotionUsages)
                .HasForeignKey(d => d.PromotionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionUsage_Promotions");

            entity.HasOne(d => d.User).WithMany(p => p.PromotionUsages)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_PromotionUsage_Users");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Roles__3214EC0710AF5767");

            entity.HasIndex(e => e.Name, "UQ__Roles__737584F6BA3ED2A8").IsUnique();

            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.Name).HasMaxLength(50);
            entity.Property(e => e.NameEn).HasMaxLength(200);
        });

        modelBuilder.Entity<SalesDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesDet__3214EC07241BF91A");

            entity.Property(e => e.Discount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([UnitPrice]*[Quantity]-[Discount])", true)
                .HasColumnType("decimal(30, 2)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesDetails_Products");

            entity.HasOne(d => d.SalesHeader).WithMany(p => p.SalesDetails)
                .HasForeignKey(d => d.SalesHeaderId)
                .HasConstraintName("FK_SalesDetails_SalesHeader");
        });

        modelBuilder.Entity<SalesDetailsReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesDet__3214EC079F104DFD");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Quantity).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.Reason).HasMaxLength(500);
            entity.Property(e => e.TotalPrice)
                .HasComputedColumnSql("([Quantity]*[UnitPrice])", true)
                .HasColumnType("decimal(37, 4)");
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Product).WithMany(p => p.SalesDetailsReturns)
                .HasForeignKey(d => d.ProductId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesDetailsReturns_Product");

            entity.HasOne(d => d.SalesHeaderReturn).WithMany(p => p.SalesDetailsReturns)
                .HasForeignKey(d => d.SalesHeaderReturnId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesDetailsReturns_Header");
        });

        modelBuilder.Entity<SalesHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesHea__3214EC0751798DAA");

            entity.ToTable("SalesHeader");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Discount).HasColumnType("decimal(18, 2)");
            entity.Property(e => e.InvoiceNumber).HasMaxLength(50);
            entity.Property(e => e.NetAmount)
                .HasComputedColumnSql("([TotalAmount]-[Discount])", true)
                .HasColumnType("decimal(19, 2)");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PaymentMethod).HasMaxLength(50);
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending");
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesHeaders)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_SalesHeader_Customers");

            entity.HasOne(d => d.Prescription).WithMany(p => p.SalesHeaders)
                .HasForeignKey(d => d.PrescriptionId)
                .HasConstraintName("FK_SalesHeader_Prescriptions");
        });

        modelBuilder.Entity<SalesHeaderReturn>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__SalesHea__3214EC07041CCBF1");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreatedBy).HasMaxLength(100);
            entity.Property(e => e.Notes).HasMaxLength(500);
            entity.Property(e => e.ReturnDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ReturnNumber).HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Customer).WithMany(p => p.SalesHeaderReturns)
                .HasForeignKey(d => d.CustomerId)
                .HasConstraintName("FK_SalesHeaderReturns_Customers");

            entity.HasOne(d => d.OriginalSalesHeader).WithMany(p => p.SalesHeaderReturns)
                .HasForeignKey(d => d.OriginalSalesHeaderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesHeaderReturns_OriginalSalesHeader");

            entity.HasOne(d => d.Pharmacy).WithMany(p => p.SalesHeaderReturns)
                .HasForeignKey(d => d.PharmacyId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_SalesHeaderReturns_Pharmacies");
        });

        //modelBuilder.Entity<Sub1Category>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PK__Sub1Cate__3214EC07CD83FB5E");

        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.ImageUrl).HasMaxLength(500);
        //    entity.Property(e => e.IsActive).HasDefaultValue(true);
        //    entity.Property(e => e.Name).HasMaxLength(200);
        //    entity.Property(e => e.NameEn).HasMaxLength(200);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

        //    entity.HasOne(d => d.Category).WithMany(p => p.Sub1Categories)
        //        .HasForeignKey(d => d.CategoryId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK_Sub1categories_CategoryId");
        //});

        //modelBuilder.Entity<Sub2Category>(entity =>
        //{
        //    entity.HasKey(e => e.Id).HasName("PK__Sub2Cate__3214EC079D628ABF");

        //    entity.Property(e => e.CreatedAt)
        //        .HasDefaultValueSql("(getdate())")
        //        .HasColumnType("datetime");
        //    entity.Property(e => e.ImageUrl).HasMaxLength(500);
        //    entity.Property(e => e.IsActive).HasDefaultValue(true);
        //    entity.Property(e => e.Name).HasMaxLength(200);
        //    entity.Property(e => e.NameEn).HasMaxLength(200);
        //    entity.Property(e => e.UpdatedAt).HasColumnType("datetime");

        //    entity.HasOne(d => d.Category).WithMany(p => p.Sub2Categories)
        //        .HasForeignKey(d => d.CategoryId)
        //        .OnDelete(DeleteBehavior.ClientSetNull)
        //        .HasConstraintName("FK_Sub2categories_CategoryId");
        //});

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Tags__3214EC07DF4476EE");

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.NameEn).HasMaxLength(100);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07DB376F24");

            entity.HasIndex(e => e.Username, "UQ__Users__536C85E4B4A0D80B").IsUnique();

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Email).HasMaxLength(150);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PasswordHash).HasMaxLength(256);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.UpdatedAt).HasColumnType("datetime");
            entity.Property(e => e.Username).HasMaxLength(100);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.RoleId }).HasName("PK__UserRole__AF2760AD06652328");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoles__RoleI__44FF419A");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__UserRoles__UserI__440B1D61");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
