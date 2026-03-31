using Lawllit.Api;
using Lawllit.Api.Finance.Services.Interfaces;
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveTheme(SaveThemeViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidThemes.Contains(form.Theme))
            return BadRequest();

        var result = await profileService.SaveThemeAsync(GetUserId(), form.Theme);
        if (!result.IsSuccess)
            return Unauthorized();

        await SignInAsync(result.Value!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveFontSize(SaveFontSizeViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidFontSizes.Contains(form.FontSize))
            return BadRequest();

        var result = await profileService.SaveFontSizeAsync(GetUserId(), form.FontSize);
        if (!result.IsSuccess)
            return Unauthorized();

        await SignInAsync(result.Value!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveLanguage(SaveLanguageViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidLanguages.Contains(form.Language))
            return BadRequest();

        var result = await profileService.SaveLanguageAsync(GetUserId(), form.Language);
        if (!result.IsSuccess)
            return Unauthorized();

        await SignInAsync(result.Value!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveCurrency(SaveCurrencyViewModel form)
    {
        if (!ModelState.IsValid || !Constants.ValidCurrencies.Contains(form.Currency))
            return BadRequest();

        var result = await profileService.SaveCurrencyAsync(GetUserId(), form.Currency);
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
