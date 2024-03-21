using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVCVentas.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVCVentas.Services;
using MVCVentas.Controllers;
using Microsoft.AspNetCore.Http;
using System.Net;

//using MVCVentas.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MVCVentasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MVCVentasContext") ?? throw new InvalidOperationException("Connection string 'MVCVentasContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Access/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        option.SlidingExpiration = true;
    });

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<AccessController>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SupervisorOrAdmin", policy => policy.RequireRole("Admin", "Supervisor"));
});

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();
