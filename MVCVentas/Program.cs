using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MVCVentas.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using MVCVentas.Services;
using MVCVentas.Controllers;
using Microsoft.AspNetCore.Http;
using System.Net;
using MVCVentas.Interfaces;
using Serilog;
using Serilog.Events;

//using MVCVentas.Data;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<MVCVentasContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MVCVentasContext") ?? throw new InvalidOperationException("Connection string 'MVCVentasContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Autenticación:
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.LoginPath = "/Access/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        option.SlidingExpiration = true;
        option.AccessDeniedPath = "/Access/AccessDenied";
    });

// Servicios:
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ILoginService, LoginService>();
builder.Services.AddScoped<AccessController>();
builder.Services.AddScoped<IVentasControllerFactory, VentasControllerFactory>();

// Roles:
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
    options.AddPolicy("SupervisorOrAdmin", policy => policy.RequireRole("Admin", "Supervisor"));
});

// Tiempo de sesión:
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(20);
});

// Crear HttpClient para llamado a apis:
builder.Services.AddHttpClient("VentasApiClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7200/api");
});

builder.Services.AddHttpClient("WSCuponesClient", client =>
{
    client.BaseAddress = new Uri("https://localhost:7159/api");
});

// Implementación de Logs
Log.Logger = new LoggerConfiguration()
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Error)
        .WriteTo.File("LogsError/Error.txt", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Information)
        .WriteTo.File("Logs/Log.txt", rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(evt => evt.Level == LogEventLevel.Warning)
        .WriteTo.File("LogsWarning/Log.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

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
