using Contracts;
using Contracts.Images.Dtos;
using Contracts.IServices;
using Entities.Models;
using Microsoft.AspNetCore.Mvc;
using Repository;
using Shared.Responses;
using WebAPI.Extensions;
using WebAPI.Filters;
using WebAPI.Middleware;
using WebAPI.Services.Security;

var builder = WebApplication.CreateBuilder(args);

// ----------------------- Services -----------------------
builder.Services.AddScoped<TraceIdResultFilter>();
builder.Services.AddMemoryCache();

// Bind options from appsettings.json
builder.Services.Configure<ImageServiceOptions>(
    builder.Configuration.GetSection("ImageService"));

builder.Services.AddImageModule(builder.Configuration);

builder.Services.AddControllers(options =>
{
    //  TraceId filter globally
    options.Filters.Add<TraceIdResultFilter>();
})
.ConfigureApiBehaviorOptions(options =>
{
    // Unified validation response -> AppResponse
    options.InvalidModelStateResponseFactory = context =>
    {
        var fieldErrors = context.ModelState
            .Where(x => x.Value?.Errors.Count > 0)
            .ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value!.Errors
                    .Select(e => string.IsNullOrWhiteSpace(e.ErrorMessage) ? "Invalid value" : e.ErrorMessage)
                    .ToArray()
            );

        var response = AppResponse.ValidationErrors(fieldErrors, detail: "Validation failed");

        // optional: set TraceId here (filter will also ensure it exists)
        response.TraceId = context.HttpContext.TraceIdentifier;

        // optional: add instance/type (good practice for RFC7807-style)
        response.Instance = context.HttpContext.Request.Path; 
        response.Type = "https://httpstatuses.com/400";        

        return new BadRequestObjectResult(response);
    };
});

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.DefaultIgnoreCondition =
        System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
});


builder.Services.AddOpenApi();

// add regester servers her
builder.Services.AddDbContextServices(builder.Configuration);
builder.Services.AddDependencyInjectionServices();
builder.Services.ConfigureJWT(builder.Configuration);
builder.Services.AddSwaggerDocumentation();

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddTransient<ExceptionHandlingMiddleware>();


var app = builder.Build();
app.UseSwaggerDocumentation(app.Environment);


app.UseMiddleware<ExceptionHandlingMiddleware>();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseStaticFiles();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
