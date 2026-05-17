using GAMINGSTORE.Authorization;
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
    public class RolePermissionController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolePermissionController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleManager.Roles.OrderBy(r => r.Name).ToListAsync();
            var model = new List<RolePermissionOverviewModel>();

            foreach (var role in roles)
            {
                var claims = await _roleManager.GetClaimsAsync(role);
                var permissionClaims = claims
                    .Where(c => c.Type == PermissionConstants.ClaimType)
                    .Select(c => c.Value)
                    .ToList();

                model.Add(new RolePermissionOverviewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name ?? string.Empty,
                    PermissionCount = permissionClaims.Count,
                    Permissions = permissionClaims
                });
            }

            return View(model);
        }

        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest();
            }

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var roleClaims = await _roleManager.GetClaimsAsync(role);
            var assignedPermissions = roleClaims
                .Where(c => c.Type == PermissionConstants.ClaimType)
                .Select(c => c.Value)
                .ToHashSet();

            var permissionItems = PermissionConstants.AllPermissions.Select(permission =>
                new PermissionItem
                {
                    Permission = permission,
                    Description = PermissionDescriptions.TryGetValue(permission, out var desc) ? desc : permission,
                    Assigned = assignedPermissions.Contains(permission)
                })
                .ToList();

            var model = new RolePermissionEditModel
            {
                RoleId = role.Id,
                RoleName = role.Name ?? string.Empty,
                Permissions = permissionItems,
                SelectedPermissions = assignedPermissions.ToList()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(RolePermissionEditModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await _roleManager.FindByIdAsync(model.RoleId);
            if (role == null)
            {
                return NotFound();
            }

            var existingClaims = await _roleManager.GetClaimsAsync(role);
            var existingPermissionClaims = existingClaims.Where(c => c.Type == PermissionConstants.ClaimType).ToList();

            foreach (var claim in existingPermissionClaims)
            {
                await _roleManager.RemoveClaimAsync(role, claim);
            }

            var selectedPermissions = model.SelectedPermissions ?? new List<string>();
            foreach (var permission in selectedPermissions)
            {
                if (!PermissionConstants.AllPermissions.Contains(permission))
                {
                    continue;
                }

                await _roleManager.AddClaimAsync(role, new Claim(PermissionConstants.ClaimType, permission));
            }

            TempData["Success"] = $"Cập nhật quyền cho role '{role.Name}' thành công.";
            return RedirectToAction(nameof(Index));
        }

        private static readonly Dictionary<string, string> PermissionDescriptions = new()
        {
            { PermissionConstants.DashboardAccess, "Truy cập Dashboard" },
            { PermissionConstants.AuditView, "Xem Audit Trail" },
            { PermissionConstants.ProductManage, "Quản lý sản phẩm" },
            { PermissionConstants.OrderManage, "Quản lý đơn hàng" },
            { PermissionConstants.OrderView, "Xem đơn hàng" },
            { PermissionConstants.ReturnManage, "Quản lý trả hàng" },
            { PermissionConstants.ReturnView, "Xem trả hàng" },
            { PermissionConstants.CouponManage, "Quản lý mã giảm giá" },
            { PermissionConstants.ReviewManage, "Quản lý đánh giá" },
            { PermissionConstants.RoleManage, "Quản lý quyền" }
        };
    }

    public class RolePermissionOverviewModel
    {
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public int PermissionCount { get; set; }
        public IEnumerable<string> Permissions { get; set; } = Enumerable.Empty<string>();
    }

    public class RolePermissionEditModel
    {
        [Required]
        public string RoleId { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public List<PermissionItem> Permissions { get; set; } = new();
        public List<string> SelectedPermissions { get; set; } = new();
    }

    public class PermissionItem
    {
        public string Permission { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool Assigned { get; set; }
    }
}
