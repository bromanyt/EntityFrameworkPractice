using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Serilog;
using WebRetail.Binder;
using WebRetail.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options => options.LoginPath = "/Account");
builder.Services.AddAuthorization();
builder.Services.AddControllersWithViews(options => options.ModelBinderProviders.Insert(0, new RetailBinderProvider()));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var logger = new LoggerConfiguration()
    .WriteTo.File($"Logs/logs_{DateTime.Now.ToString("dd-MM-yy-hh-mm-ss")}.txt", rollingInterval: RollingInterval.Infinite)
    .CreateLogger();
builder.Services.AddSingleton<Serilog.ILogger>(logger);
builder.Services.AddDbContext<RetailContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
