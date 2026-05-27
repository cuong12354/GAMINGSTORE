using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using GAMINGSTORE.Repositories;
using GAMINGSTORE.Seeding;
using GAMINGSTORE.Services;
using GAMINGSTORE.Authorization;
using GAMINGSTORE.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

var dataProtectionKeyPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "GAMINGSTORE",
    "DataProtection-Keys");

Directory.CreateDirectory(dataProtectionKeyPath);

builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(dataProtectionKeyPath))
    .SetApplicationName("GAMINGSTORE");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));


builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.SignIn.RequireConfirmedAccount = false; // 🔥 thêm dòng này
})
.AddDefaultTokenProviders()
.AddDefaultUI()
.AddEntityFrameworkStores<ApplicationDbContext>();

// ===== OAUTH2 EXTERNAL LOGIN PROVIDERS =====
var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
var googleClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];

// Only add Google authentication if credentials are configured
if (!string.IsNullOrEmpty(googleClientId) && !string.IsNullOrEmpty(googleClientSecret))
{
    builder.Services.AddAuthentication()
        .AddGoogle(options =>
        {
            options.ClientId = googleClientId;
            options.ClientSecret = googleClientSecret;
        });
}
// Google login disabled - no credentials configured

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = ".GAMINGSTORE.Auth";
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied"; 
});

builder.Services.AddRazorPages();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.Name = ".GAMINGSTORE.Session.v2";
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PermissionConstants.DashboardAccess, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.DashboardAccess)));

    options.AddPolicy(PermissionConstants.AuditView, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.AuditView)));

    options.AddPolicy(PermissionConstants.ProductManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.ProductManage)));

    options.AddPolicy(PermissionConstants.OrderManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.OrderManage)));

    options.AddPolicy(PermissionConstants.OrderView, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.OrderView)));

    options.AddPolicy(PermissionConstants.ReturnManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.ReturnManage)));

    options.AddPolicy(PermissionConstants.ReturnView, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.ReturnView)));

    options.AddPolicy(PermissionConstants.CouponManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.CouponManage)));

    options.AddPolicy(PermissionConstants.ReviewManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.ReviewManage)));

    options.AddPolicy(PermissionConstants.RoleManage, policy =>
        policy.RequireAssertion(context => context.User.HasPermission(PermissionConstants.RoleManage)));
});

// Add services to the container.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // Configure JSON serializer to ignore circular references
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true;
    });

builder.Services.AddScoped<IProductRepository, EFProductRepository>();
builder.Services.AddScoped<ICategoryRepository, EFCategoryRepository>();
builder.Services.AddScoped<IRecommendationService, RecommendationService>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IWishlistRepository, WishlistRepository>();
builder.Services.AddScoped<ICouponRepository, CouponRepository>();
builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();
builder.Services.AddScoped<ILoyaltyService, LoyaltyService>();
builder.Services.AddScoped<IReturnService, ReturnService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IOrderManagementService, OrderManagementService>();
builder.Services.AddScoped<IAuditService, AuditService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IGeminiService, GeminiService>();
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IVnpayService, VnpayService>();

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

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

app.MapRazorPages();

// ===== SEED DATABASE ON STARTUP =====
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    // Seed roles and admin user
    await RoleSeeding.SeedRolesAndAdminAsync(dbContext, userManager, roleManager);
    
    // Seed products and categories
    // await DataSeeding.SeedCategoriesAndProducts(dbContext);
}

app.Run();
