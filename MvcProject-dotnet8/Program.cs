using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MvcProject_dotnet8.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Add developer page exception filter.
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configure Identity with roles.
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>() // Shtimi i roleve
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Add controllers with views.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // The default HSTS value is 30 days.
}

// Middleware për konfigurimet e aplikacionit.
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Add authentication and authorization middleware.
app.UseAuthentication(); // Siguron që autentifikimi të jetë aktiv.
app.UseAuthorization();

// Configure routes.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
