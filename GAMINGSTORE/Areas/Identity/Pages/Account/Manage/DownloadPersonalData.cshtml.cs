// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System.Collections.Generic;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GAMINGSTORE.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace GAMINGSTORE.Areas.Identity.Pages.Account.Manage
{
    public class DownloadPersonalDataModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<DownloadPersonalDataModel> _logger;

        public DownloadPersonalDataModel(UserManager<ApplicationUser> userManager, ILogger<DownloadPersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return RedirectToPage("./PersonalData");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            _logger.LogInformation("User with ID '{UserId}' requested their personal data.", _userManager.GetUserId(User));

            var personalData = new Dictionary<string, string>
            {
                ["UserName"] = user.UserName ?? string.Empty,
                ["Email"] = user.Email ?? string.Empty,
                ["PhoneNumber"] = user.PhoneNumber ?? string.Empty,
                ["FullName"] = user.FullName ?? string.Empty,
                ["Address"] = user.Address ?? string.Empty,
                ["Age"] = user.Age ?? string.Empty
            };

            var fileBytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(personalData, new JsonSerializerOptions { WriteIndented = true }));
            return File(fileBytes, "application/json", "PersonalData.json");
        }
    }
}