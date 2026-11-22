using Contracts;
using Contracts.Images.Abstractions;
using Contracts.Images.Dtos;
using Contracts.IServices;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Repository.Identity;
using Service;
using Service.Auth;
using Service.Images;
using Shared.Constants;
using Shared.Mappings;
using System.Text;
using System.Security.Claims;
using WebAPI.Notifications;
namespace WebAPI.Extensions;

public static class ServiceExtensions
{
    public static void AddDependencyInjectionServices(this IServiceCollection services)
    {
        //services.AddTransient(typeof(IGenericRepository<Customer>), typeof(GenericRepository<Customer>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<Contracts.IServices.IMyImageService, Service.MyImageService>();
        services.AddScoped<JwtTokenService>();
        services.AddScoped<ILoginAuditService, LoginAuditService>();
        services.AddScoped<IUserRoleService, UserRoleService>();
        services.AddScoped<IUserPermissionService, UserPermissionService>();

        //services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        //services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<ICustomerService, CustomerService>();
        services.AddTransient<ICustomerAddressService, CustomerAddressService>();
        services.AddTransient<IProductService, ProductService>();
        services.AddTransient<ICategoryService, CategoryService>();
        services.AddTransient<ITagService, TagService>();
        services.AddTransient<IReferenceDataService, ReferenceDataService>();
        services.AddTransient<ICartService, CartService>();
        services.AddTransient<IPromotionService, PromotionService>();
        services.AddTransient<IBannerService, BannerService>();
        services.AddTransient<IOrderService, OrderService>();
        services.AddTransient<IPrescriptionService, PrescriptionService>();
        services.AddTransient<IPointsService, PointsService>();
        services.AddTransient<IInventoryService, InventoryService>();
        services.AddTransient<IReturnRequestService, ReturnRequestService>();
        services.AddTransient<IDeliveryService, DeliveryService>();
        services.AddTransient<IPharmacyService, PharmacyService>();
        services.AddTransient<IStoreService, StoreService>();
        services.AddTransient<IEmployeeService, EmployeeService>();
        services.AddTransient<ILocationService, LocationService>();
        services.AddTransient<IDeliveryZoneService, DeliveryZoneService>();
        services.AddTransient<INotificationService, NotificationService>();
        services.AddTransient<IPaymentService, PaymentService>();


        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<Contracts.Notifications.IEmailSender>(sp =>
            string.IsNullOrWhiteSpace(sp.GetRequiredService<IConfiguration>()["Smtp:Host"])
                || string.IsNullOrWhiteSpace(sp.GetRequiredService<IConfiguration>()["Smtp:From"])
                ? new LoggingEmailSender(sp.GetRequiredService<ILogger<LoggingEmailSender>>())
                : new SmtpEmailSender(sp.GetRequiredService<IConfiguration>()));

    }

    public static void AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        var conn = configuration.GetConnectionString("DefaultConnection");

        // Domain DB context (reverse engineered)
        services.AddDbContext<RepositoryContext>(options =>
            options.UseSqlServer(conn));

        // Identity DB context migrations
        services.AddDbContext<IdentityDbContext>(options =>
            options.UseSqlServer(conn));

        // ---------------------------------------------
        // Identity
        // ---------------------------------------------
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password policy
            options.Password.RequiredLength = 8;
            options.Password.RequireDigit = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireNonAlphanumeric = true;

            // Lockout
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.AllowedForNewUsers = true;

            // User & SignIn
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        })
        .AddEntityFrameworkStores<IdentityDbContext>()
        .AddDefaultTokenProviders(); // EmailConfirm + ResetPassword
    }


    public static IServiceCollection AddImageModule(this IServiceCollection services, IConfiguration configuration)
    {
        //services.Configure(configure);

        // Bind options from appsettings.json
        //services.Configure<ImageServiceOptions>(configure.GetSection("ImageService"));
        //services.AddImageModule(opt =>
        //{
        //    opt.MaxUploadBytes = 10 * 1024 * 1024;
        //    opt.MediumWidth = 1000;
        //    opt.SmallWidth = 300;
        //    opt.PreferWebpWhenAlpha = true;
        //});
        // Processor (ImageSharp) + Local Storage
        services.AddSingleton<IImageProcessor, ImageSharpProcessor>();
        services.AddSingleton<IImageStorage, LocalDiskImageStorage>();

        // Orchestrator
        services.AddSingleton<Contracts.Images.Abstractions.IImageService, Service.Images.ImageService>();


       

        return services;

    }


    public static void ConfigureJWT(this IServiceCollection services, IConfiguration configuration)
    {
        var jwt = configuration.GetSection("Jwt");
        var key = Encoding.UTF8.GetBytes(jwt["Key"]!);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwt["Issuer"],
                ValidAudience = jwt["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
                RoleClaimType = ClaimTypes.Role,

                ClockSkew = TimeSpan.FromSeconds(30)
            };
        });
        services.AddAuthorization(options =>
        {
            options.AddPolicy(AppPolicies.Admin, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.Admin));

            options.AddPolicy(AppPolicies.Customer, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.Customer));

            options.AddPolicy(AppPolicies.Employee, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.Employee));

            options.AddPolicy(AppPolicies.DeliveryAgent, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.DeliveryAgent));

            options.AddPolicy(AppPolicies.CanManageUsers, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.Admin));

            options.AddPolicy(AppPolicies.CanManageStores, policy =>
                policy.RequireAuthenticatedUser().RequireRole(AppRoles.Admin));

            foreach (var permission in AppPermissions.PolicyPermissions)
            {
                options.AddPolicy(permission.Key, policy => policy
                    .RequireAuthenticatedUser()
                    .RequireAssertion(context =>
                        context.User.IsInRole(AppRoles.Admin) ||
                        context.User.HasClaim(AppPermissions.ClaimType, permission.Value)));
            }
        });



        //services.AddAuthentication(opt =>
        //{
        //    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        //    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        //}).AddJwtBearer(options =>
        //{
        //    options.TokenValidationParameters = new TokenValidationParameters
        //    {
        //        ValidateIssuerSigningKey = true,
        //        ValidateIssuer = true,
        //        ValidateAudience = true,
        //        ValidateLifetime = true,
        //        RoleClaimType = ClaimTypes.Role,
        //        ValidIssuer = "PharmaInventoryMobileApp",
        //        ValidAudience = "Pharmacist",
        //        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sz8eI7OdHBrjrIo8j9nTW/rQyO1OvY0pAQ2wDKQZw/0="))
        //    };
        //});
    }
}
