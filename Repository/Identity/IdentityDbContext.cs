using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repository.Identity;


public class IdentityDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    // dotnet ef migrations add InitIdentity --project Repository --startup-project WebAPI --context Repository.Identity.IdentityDbContext --output-dir Migrations/Identity
    // dotnet ef migrations script --project Repository --startup-project WebAPI --context Repository.Identity.IdentityDbContext -o identity.sql


    public DbSet<LoginAudit> LoginAudits => Set<LoginAudit>();

    public IdentityDbContext(DbContextOptions<IdentityDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //  نفس collation الخاص بقاعدتك (اختياري لكن أنصح به إذا DB عربية)
        builder.UseCollation("Arabic_100_CI_AS_KS_WS_SC_UTF8");

        //  (اختياري) تخصيص أسماء الجداول
        // أنصح بالبقاء على AspNet* الآن لتفادي مشاكل
        // لو أردت schema منفصل:
        // builder.HasDefaultSchema("auth");

        //  (اختياري) إضافة index أو تعديل lengths عند الحاجة لاحقًا

        // لو تريد Role properties الإضافية تكون لها lengths
        builder.Entity<ApplicationRole>(entity =>
        {
            entity.Property(x => x.NameEn).HasMaxLength(200);
            entity.Property(x => x.Description).HasMaxLength(500);
        });

        builder.Entity<LoginAudit>(entity =>
        {
            entity.ToTable("LoginAudits", "dbo");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreatedAtUtc)
                .HasColumnType("datetime2(3)");

            entity.Property(x => x.Outcome)
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.FailureReason)
                .HasMaxLength(50);

            entity.Property(x => x.IdentifierMasked)
                .HasMaxLength(256);

            entity.Property(x => x.IdentifierHash)
                .HasMaxLength(32);

            entity.Property(x => x.IpAddress)
                .HasMaxLength(45);

            entity.Property(x => x.UserAgent)
                .HasMaxLength(512);

            entity.Property(x => x.TraceId)
                .HasMaxLength(64);
        });
    }
}
