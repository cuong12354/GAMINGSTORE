using GAMINGSTORE.Authorization;
using GAMINGSTORE.Data;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace GAMINGSTORE.Seeding
{
    public class RoleSeeding
    {
        /// <summary>
        /// Seed roles and admin user
        /// </summary>
        public static async Task SeedRolesAndAdminAsync(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            try
            {
                // ============ SEED ROLES ============
                var roles = new[] { "Admin", "Employee", "Customer", "Company" };

                foreach (var roleName in roles)
                {
                    var roleExist = await roleManager.RoleExistsAsync(roleName);
                    if (!roleExist)
                    {
                        var result = await roleManager.CreateAsync(new IdentityRole(roleName));
                        if (!result.Succeeded)
                        {
                            throw new Exception($"Lỗi tạo role '{roleName}': {string.Join(",", result.Errors.Select(e => e.Description))}");
                        }
                    }
                }

                // ============ SEED ROLE PERMISSIONS ============
                var rolePermissions = new Dictionary<string, string[]>
                {
                    ["Admin"] = PermissionConstants.AllPermissions,
                    ["Employee"] = new[]
                    {
                        PermissionConstants.ProductManage,
                        PermissionConstants.OrderView,
                        PermissionConstants.OrderManage,
                        PermissionConstants.ReturnView,
                        PermissionConstants.ReturnManage,
                        PermissionConstants.DashboardAccess,
                        PermissionConstants.CouponManage,
                        PermissionConstants.ReviewManage
                    },
                    ["Company"] = new[]
                    {
                        PermissionConstants.OrderView,
                        PermissionConstants.DashboardAccess
                    },
                    ["Customer"] = new[]
                    {
                        PermissionConstants.OrderView,
                        PermissionConstants.ReturnView
                    }
                };

                foreach (var (roleName, permissions) in rolePermissions)
                {
                    var role = await roleManager.FindByNameAsync(roleName);
                    if (role == null)
                    {
                        continue;
                    }

                    var existingClaims = await roleManager.GetClaimsAsync(role);
                    foreach (var permission in permissions)
                    {
                        if (!existingClaims.Any(c => c.Type == PermissionConstants.ClaimType && c.Value == permission))
                        {
                            await roleManager.AddClaimAsync(role, new Claim(PermissionConstants.ClaimType, permission));
                        }
                    }
                }

                // ============ SEED ADMIN USER ============
                const string adminEmail = "admin@gamingstore.com";
                const string adminPassword = "Admin@123!"; // 8+ chars, uppercase, lowercase, digit, special
                const string adminFullName = "Game Store Administrator";

                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    // Get default tier (Đồng tier with Id=1)
                    var defaultTier = await context.MemberTiers.FirstOrDefaultAsync(t => t.Name == "Đồng");
                    
                    adminUser = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        FullName = adminFullName,
                        Age = "30",
                        Address = "123 Gaming Lane, Tech City",
                        MemberTierId = defaultTier?.Id ?? 1  // Use Đồng tier or default to 1
                    };

                    var result = await userManager.CreateAsync(adminUser, adminPassword);

                    if (!result.Succeeded)
                    {
                        throw new Exception($"Lỗi tạo admin user: {string.Join(",", result.Errors.Select(e => e.Description))}");
                    }

                    // Assign Admin role
                    await userManager.AddToRoleAsync(adminUser, "Admin");

                    System.Console.WriteLine($"✅ Đã tạo admin user: {adminEmail}");
                    System.Console.WriteLine($"📝 Mật khẩu: {adminPassword}");
                    System.Console.WriteLine($"🔓 Đăng nhập: admin@gamingstore.com / Admin@123!");
                }
                else
                {
                    // Ensure admin user has Admin role
                    var isInRole = await userManager.IsInRoleAsync(adminUser, "Admin");
                    if (!isInRole)
                    {
                        await userManager.AddToRoleAsync(adminUser, "Admin");
                    }
                }

                await context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"❌ Lỗi trong RoleSeeding: {ex.Message}");
                throw;
            }
        }
    }
}
