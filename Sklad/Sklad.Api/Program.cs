using Sklad.Persistence;
using Microsoft.EntityFrameworkCore;
using Sklad.Domain.Interfaces;
using Sklad.Application.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// Add services to the container.
builder.Services.AddDbContext<SkladDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ILogger, Logger<SkladDbContext>>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IStorageService, StorageService>();
var app = builder.Build();

// Configure the HTTP request pipeline.  
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.  
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowFrontend");

app.MapControllers();
//app.UseAuthorization();

app.Run();
