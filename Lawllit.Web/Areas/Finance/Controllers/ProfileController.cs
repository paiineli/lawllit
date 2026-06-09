using Lawllit.Api;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class ProfileController(IProfileService profileService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(string? tab)
    {
        var viewModel = await profileService.GetProfileAsync(GetUserId(), tab);
        if (viewModel is null)
            return RedirectToAction("Logout", "Auth");

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditName(EditNameViewModel editNameForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = GetFirstModelError("Msg_NameInvalid");
            return RedirectToAction("Index");
        }

        var result = await profileService.EditNameAsync(GetUserId(), editNameForm.Name);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index");
        }

        await SignInAsync(result.Value!);
        TempData["Success"] = localizer["Msg_NameUpdated"].Value;
        return RedirectToAction("Index");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmail(EditEmailViewModel editEmailForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = GetFirstModelError("Msg_EmailInvalid");
            return RedirectToAction("Index", new { tab = "security" });
        }

        var result = await profileService.EditEmailAsync(GetUserId(), editEmailForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        await SignInAsync(result.Value!);
        TempData["Success"] = localizer["Msg_EmailUpdated"].Value;
        return RedirectToAction("Index", new { tab = "security" });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel changePasswordForm)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = GetFirstModelError("Msg_DataInvalid");
            return RedirectToAction("Index", new { tab = "security" });
        }

        var result = await profileService.ChangePasswordAsync(GetUserId(), changePasswordForm);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index", new { tab = "security" });
        }

        TempData["Success"] = localizer["Msg_PasswordChanged"].Value;
        return RedirectToAction("Index", new { tab = "security" });
    }

    private static readonly Dictionary<string, (string[] ValidValues, Action<User, string> Apply)> Preferences = new()
    {
        ["theme"]    = (Constants.ValidThemes,     static (user, value) => user.Theme = value),
        ["fontSize"] = (Constants.ValidFontSizes,  static (user, value) => user.FontSize = value),
        ["language"] = (Constants.ValidLanguages,  static (user, value) => user.Language = value),
        ["currency"] = (Constants.ValidCurrencies, static (user, value) => user.Currency = value),
    };

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePreference(string key, string value)
    {
        if (key is null || !Preferences.TryGetValue(key, out var preference) || !preference.ValidValues.Contains(value))
            return BadRequest();

        var result = await profileService.UpdatePreferenceAsync(GetUserId(), user => preference.Apply(user, value));
        if (!result.IsSuccess)
            return Unauthorized();

        await SignInAsync(result.Value!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string? password)
    {
        var result = await profileService.DeleteAccountAsync(GetUserId(), password);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Index", new { tab = "account" });
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    private string GetFirstModelError(string fallbackKey) =>
        ModelState.Values
            .SelectMany(value => value.Errors)
            .FirstOrDefault()?.ErrorMessage ?? localizer[fallbackKey].Value;
}
