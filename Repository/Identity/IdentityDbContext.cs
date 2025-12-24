using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Repository.Identity;


public class IdentityDbContext: IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    // dotnet ef migrations add InitIdentity --project Repository --startup-project WebAPI --context Repository.Identity.IdentityDbContext --output-dir Migrations/Identity
    // dotnet ef migrations script --project Repository --startup-project WebAPI --context Repository.Identity.IdentityDbContext -o identity.sql



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
    }
}
