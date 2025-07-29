using Microsoft.OpenApi.Models;

namespace WebAPI.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.OrderActionsBy((apiDesc) => $"{apiDesc.ActionDescriptor.AttributeRouteInfo?.Order}");

            options.SwaggerDoc("Pharma", new OpenApiInfo { Title = "Pharma Store", Version = "v1" });
            options.SwaggerDoc("Inventory", new OpenApiInfo { Title = "Pharma Inventory", Version = "v1" });

            options.DocInclusionPredicate((documentName, apiDesc) =>
                apiDesc.GroupName == documentName);

            options.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

            // Add the security definition
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter your JWT token in the format: Bearer {your token}"
            });

            // Add the security requirement
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

        });

        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseSwagger(c =>
        {
            c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
            {
                httpReq.HttpContext.Response.Headers["Cache-Control"] = "no-store, no-cache, must-revalidate, proxy-revalidate";
                httpReq.HttpContext.Response.Headers["Pragma"] = "no-cache";
                httpReq.HttpContext.Response.Headers["Expires"] = "0";
                httpReq.HttpContext.Response.Headers["Surrogate-Control"] = "no-store";
            });
            
        });

        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/Pharma/swagger.json", "Pharma Partners");
            options.RoutePrefix = "swagger/pharma";
        });

        app.Map("/swagger/beta", betaApp =>
        {
            betaApp.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/inventory/swagger.json", "Pharma Inventory");
                options.RoutePrefix = "";
            });
        });
        return app;
    }
}
