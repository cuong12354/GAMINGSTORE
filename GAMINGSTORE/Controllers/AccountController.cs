using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GAMINGSTORE.Controllers
{
    public class AccountController : Controller
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(
            string provider,
            string returnUrl = null)
        {
            var redirectUrl = Url.Action(
                nameof(ExternalLoginCallback),
                "Account",
                new { returnUrl });

            var properties =
                _signInManager
                .ConfigureExternalAuthenticationProperties(
                    provider,
                    redirectUrl);

            return Challenge(properties, provider);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(
            string returnUrl = null,
            string remoteError = null)
        {
            returnUrl ??= Url.Content("~/");

            if (remoteError != null)
            {
                TempData["Error"] = remoteError;
                return RedirectToAction("Login");
            }

            var info =
                await _signInManager.GetExternalLoginInfoAsync();

            if (info == null)
            {
                return RedirectToAction("Login");
            }

            var result =
                await _signInManager.ExternalLoginSignInAsync(
                    info.LoginProvider,
                    info.ProviderKey,
                    false);

            if (result.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }

            var email =
                info.Principal.FindFirstValue(ClaimTypes.Email);

            if (email == null)
            {
                TempData["Error"] =
                    "Không lấy được email từ Google";

                return RedirectToAction("Login");
            }

            var user =
                await _userManager.FindByEmailAsync(email);

            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email
                };

                var createResult =
                    await _userManager.CreateAsync(user);

                if (!createResult.Succeeded)
                {
                    TempData["Error"] =
                        "Không tạo được tài khoản";

                    return RedirectToAction("Login");
                }
            }

            var loginResult =
                await _userManager.AddLoginAsync(user, info);

            await _signInManager.SignInAsync(
                user,
                isPersistent: false);

            return LocalRedirect(returnUrl);
        }
    }
}