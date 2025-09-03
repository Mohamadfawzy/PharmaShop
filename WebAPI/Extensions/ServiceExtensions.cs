using System.Security.Claims;
using System.Text;
using Contracts;
using Contracts.IServices;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository;
using Service;

namespace WebAPI.Extensions;

public static class ServiceExtensions
{
    public static void AddDependencyInjectionServices(this IServiceCollection services)
    {
        //services.AddTransient(typeof(IGenericRepository<Customer>), typeof(GenericRepository<Customer>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ImageService>();

        //services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        //services.AddTransient<ICustomerRepository, CustomerRepository>();
        services.AddTransient<ICustomerService, CustomerService>();

    }

    public static void AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<RepositoryContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));
    }

    public static void ConfigureJWT(this IServiceCollection services)
    {
        services.AddAuthentication(opt =>
        {
            opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                RoleClaimType = ClaimTypes.Role,
                ValidIssuer = "PharmaInventoryMobileApp",
                ValidAudience = "Pharmacist",
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("sz8eI7OdHBrjrIo8j9nTW/rQyO1OvY0pAQ2wDKQZw/0="))
            };
        });
    }


}