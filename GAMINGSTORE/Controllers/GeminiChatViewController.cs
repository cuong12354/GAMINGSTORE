using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GAMINGSTORE.Controllers
{
    [Authorize]
    public class GeminiChatViewerController : Controller
    {
        public IActionResult Index()
        {
            return View("~/Views/GeminiChat/Index.cshtml");
        }
    }
}
