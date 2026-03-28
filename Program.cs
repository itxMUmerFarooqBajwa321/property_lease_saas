using System.Data;
using Microsoft.Data.Sqlite;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using property_lease_saas.Data;
using Microsoft.AspNetCore.Identity.UI.Services;
using property_lease_saas.Services;
using property_lease_saas.Models.Repositories;
using property_lease_saas.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
// Add these using statements
using property_lease_saas.Hubs; // We'll create this folder

var builder = WebApplication.CreateBuilder(args);

// Add SignalR service
builder.Services.AddSignalR();
builder.Services.AddScoped<INotificationService, NotificationService>();

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

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // session timeout
    options.SlidingExpiration = true;                 // don't auto-renew
    options.Cookie.IsEssential = true;
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;    
    options.Cookie.MaxAge = null; // <-- key line
});

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
builder.Services.AddScoped<IFileStorage, LocalFileStorage>();
builder.Services.AddScoped<IMaintenanceRepository, MaintenanceRepository>();
builder.Services.AddScoped<IMaintenanceApplicationRepository, MaintenanceApplicationRepository>();
builder.Services.AddScoped<MaintenanceService>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<DashboardService>();  
builder.Services.AddScoped<IMechanicApplicationService, MechanicApplicationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

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
    //pattern: "Identity/Account/Login");
    //pattern: "{area:exists}/{controller=Account}/{action=Login}/{id?}");
app.MapRazorPages(); // 🔴 REQUIRED for /Identity/*

// Add SignalR Hub route
app.MapHub<NotificationHub>("/notificationHub");

app.Run();