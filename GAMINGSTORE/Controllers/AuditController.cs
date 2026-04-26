using GAMINGSTORE.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AuditController : Controller
    {
        private readonly IAuditService _auditService;

        public AuditController(IAuditService auditService)
        {
            _auditService = auditService;
        }

        public async Task<IActionResult> Index(int pageNumber = 1)
        {
            int pageSize = 20;
            var logs = await _auditService.GetAuditLogsAsync(pageNumber: pageNumber, pageSize: pageSize);
            var totalCount = await _auditService.GetTotalAuditLogsCountAsync();

            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalCount / pageSize);
            ViewBag.TotalCount = totalCount;

            return View(logs);
        }

        public async Task<IActionResult> ByEntity(string entityName, int pageNumber = 1)
        {
            int pageSize = 20;
            var logs = await _auditService.GetAuditLogsAsync(entityName: entityName, pageNumber: pageNumber, pageSize: pageSize);

            ViewBag.EntityName = entityName;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)logs.Count / pageSize);

            return View("Index", logs);
        }

        public async Task<IActionResult> ByUser(string userId, int pageNumber = 1)
        {
            int pageSize = 20;
            var logs = await _auditService.GetUserAuditLogsAsync(userId, pageNumber, pageSize);

            ViewBag.UserId = userId;
            ViewBag.CurrentPage = pageNumber;
            ViewBag.TotalPages = (int)Math.Ceiling((double)logs.Count / pageSize);

            return View("Index", logs);
        }

        public async Task<IActionResult> EntityHistory(string entityName, int entityId)
        {
            var logs = await _auditService.GetEntityAuditLogsAsync(entityName, entityId);

            ViewBag.EntityName = entityName;
            ViewBag.EntityId = entityId;

            return View(logs);
        }

        [HttpPost]
        public async Task<IActionResult> ClearOldLogs(int daysToKeep = 90)
        {
            try
            {
                await _auditService.ClearOldAuditLogsAsync(daysToKeep);
                TempData["Success"] = $"✅ Xoá các log cũ hơn {daysToKeep} ngày thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"❌ Lỗi: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
