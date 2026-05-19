using GAMINGSTORE.Authorization;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace GAMINGSTORE.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = PermissionConstants.RoleManage)]
    public class UserPermissionController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserPermissionController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index(string? search)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                usersQuery = usersQuery.Where(u =>
                    (u.UserName != null && u.UserName.Contains(search)) ||
                    (u.Email != null && u.Email.Contains(search)) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search))
                );
            }

            var users = await usersQuery
                .OrderBy(u => u.UserName)
                .ToListAsync();

            var model = new List<UserPermissionOverviewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var claims = await _userManager.GetClaimsAsync(user);

                var directPermissions = claims
                    .Where(c => c.Type == PermissionConstants.ClaimType)
                    .Select(c => c.Value)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                model.Add(new UserPermissionOverviewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    PhoneNumber = user.PhoneNumber ?? "",
                    Roles = roles.ToList(),
                    DirectPermissionCount = directPermissions.Count,
                    DirectPermissions = directPermissions
                });
            }

            ViewBag.Search = search ?? "";
            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var allRoles = await _roleManager.Roles
                .OrderBy(r => r.Name)
                .Select(r => r.Name ?? "")
                .Where(r => r != "")
                .ToListAsync();

            var userRoles = await _userManager.GetRolesAsync(user);
            var userClaims = await _userManager.GetClaimsAsync(user);

            var directPermissions = userClaims
                .Where(c => c.Type == PermissionConstants.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            var permissionItems = PermissionConstants.AllPermissions.Select(permission =>
                new UserPermissionItem
                {
                    Permission = permission,
                    Description = PermissionDescriptions.TryGetValue(permission, out var desc) ? desc : permission,
                    Assigned = directPermissions.Contains(permission)
                })
                .ToList();

            var model = new UserPermissionEditModel
            {
                UserId = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber ?? "",
                AllRoles = allRoles,
                SelectedRoles = userRoles.ToList(),
                Permissions = permissionItems,
                SelectedPermissions = directPermissions.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserPermissionEditModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
            {
                return NotFound();
            }

            model.SelectedRoles ??= new List<string>();
            model.SelectedPermissions ??= new List<string>();

            var currentRoles = await _userManager.GetRolesAsync(user);

            var rolesToRemove = currentRoles.Except(model.SelectedRoles).ToList();
            var rolesToAdd = model.SelectedRoles.Except(currentRoles).ToList();

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                {
                    foreach (var error in removeResult.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }

            foreach (var role in rolesToAdd)
            {
                if (await _roleManager.RoleExistsAsync(role))
                {
                    var addRoleResult = await _userManager.AddToRoleAsync(user, role);
                    if (!addRoleResult.Succeeded)
                    {
                        foreach (var error in addRoleResult.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                    }
                }
            }

            var existingClaims = await _userManager.GetClaimsAsync(user);
            var existingPermissionClaims = existingClaims
                .Where(c => c.Type == PermissionConstants.ClaimType)
                .ToList();

            foreach (var claim in existingPermissionClaims)
            {
                await _userManager.RemoveClaimAsync(user, claim);
            }

            foreach (var permission in model.SelectedPermissions.Distinct())
            {
                if (!PermissionConstants.AllPermissions.Contains(permission))
                {
                    continue;
                }

                await _userManager.AddClaimAsync(
                    user,
                    new Claim(PermissionConstants.ClaimType, permission)
                );
            }

            if (!ModelState.IsValid)
            {
                model.AllRoles = await _roleManager.Roles
                    .OrderBy(r => r.Name)
                    .Select(r => r.Name ?? "")
                    .Where(r => r != "")
                    .ToListAsync();

                model.Permissions = PermissionConstants.AllPermissions.Select(permission =>
                    new UserPermissionItem
                    {
                        Permission = permission,
                        Description = PermissionDescriptions.TryGetValue(permission, out var desc) ? desc : permission,
                        Assigned = model.SelectedPermissions.Contains(permission)
                    })
                    .ToList();

                return View(model);
            }

            TempData["Success"] = $"✅ Đã cập nhật quyền cho tài khoản {user.UserName}.";
            return RedirectToAction(nameof(Index));
        }

        private static readonly Dictionary<string, string> PermissionDescriptions = new()
        {
            { PermissionConstants.DashboardAccess, "Truy cập Dashboard" },
            { PermissionConstants.AuditView, "Xem nhật ký hệ thống" },
            { PermissionConstants.ProductManage, "Quản lý sản phẩm" },
            { PermissionConstants.OrderManage, "Quản lý đơn hàng" },
            { PermissionConstants.OrderView, "Xem đơn hàng" },
            { PermissionConstants.ReturnManage, "Quản lý trả hàng" },
            { PermissionConstants.ReturnView, "Xem trả hàng" },
            { PermissionConstants.CouponManage, "Quản lý mã giảm giá" },
            { PermissionConstants.ReviewManage, "Quản lý đánh giá" },
            { PermissionConstants.RoleManage, "Quản lý phân quyền" }
        };
    }

    public class UserPermissionOverviewModel
    {
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public List<string> Roles { get; set; } = new();
        public int DirectPermissionCount { get; set; }
        public List<string> DirectPermissions { get; set; } = new();
    }

    public class UserPermissionEditModel
    {
        [Required]
        public string UserId { get; set; } = "";
        public string UserName { get; set; } = "";
        public string Email { get; set; } = "";
        public string PhoneNumber { get; set; } = "";

        public List<string> AllRoles { get; set; } = new();
        public List<string> SelectedRoles { get; set; } = new();

        public List<UserPermissionItem> Permissions { get; set; } = new();
        public List<string> SelectedPermissions { get; set; } = new();
    }

    public class UserPermissionItem
    {
        public string Permission { get; set; } = "";
        public string Description { get; set; } = "";
        public bool Assigned { get; set; }
    }
}
