using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using property_lease_saas.Services;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Models;


var builder = WebApplication.CreateBuilder(args);


// Repository registrations
builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();
builder.Services.AddScoped<LeaseRepository>();

// Service registrations
builder.Services.AddScoped<PropertyService>();
builder.Services.AddScoped<LeaseService>();

/// ===============================
/// DATABASE (Identity → EF Core)
/// ===============================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


/// ===============================
/// IDENTITY (REQUIRED)
/// ===============================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddTransient<IEmailSender, EmailSender>();


builder.Services.AddAuthorization(options =>
{

    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("UserType", "Admin"));

    options.AddPolicy("LandlordOnly", policy =>
        policy.RequireClaim("UserType", "Landlord"));

    options.AddPolicy("TenantOnly", policy =>
        policy.RequireClaim("UserType", "Tenant"));

    options.AddPolicy("MechanicOnly", policy =>
        policy.RequireClaim("UserType", "Mechanic"));
});



/// ===============================
/// MVC + RAZOR PAGES (REQUIRED)
/// ===============================
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();


/// ===============================
/// DAPPER SUPPORT
/// ===============================
builder.Services.AddScoped<IDbConnection>(sp =>
{
    var cs = builder.Configuration.GetConnectionString("DefaultConnection")
             ?? throw new InvalidOperationException("DefaultConnection is missing.");
    return new SqliteConnection(cs);
});

builder.Services.AddScoped<PropertyService>();

builder.Services.AddScoped<IPropertyRepository, PropertyRepository>();

builder.Services.AddScoped<LeaseRepository>();

builder.Services.AddScoped<IFileStorage, LocalFileStorage>();

builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IMaintenanceApplicationRepository, MaintenanceApplicationRepository>();

builder.Services.AddScoped<MaintenanceService>();


var app = builder.Build();


/// ===============================
/// PIPELINE
/// ===============================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // 🔴 REQUIRED
app.UseAuthorization();


/// ===============================
/// ENDPOINTS (VERY IMPORTANT)
/// ===============================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");

app.MapRazorPages(); // 🔴 REQUIRED for /Identity/*

app.Run();