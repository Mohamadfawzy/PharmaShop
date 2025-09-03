using Contracts;
using Entities.Models;
using Repository;
using WebAPI.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// add regester servers her
builder.Services.AddDbContextServices(builder.Configuration);
builder.Services.AddDependencyInjectionServices();
builder.Services.ConfigureJWT();
builder.Services.AddSwaggerDocumentation();





var app = builder.Build();
app.UseSwaggerDocumentation(app.Environment);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseStaticFiles();
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
