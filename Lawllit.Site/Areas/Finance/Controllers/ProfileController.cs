using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Finance;
using Lawllit.Site.Models.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class ProfileController(IProfileREP profileRepository, IStringLocalizer<SharedResource> localizer) : BaseController
{
    private const string SecurityTab = "security";
    private const string AccountTab = "account";

    [HttpGet]
    public async Task<IActionResult> Index(string? tab, CancellationToken cancellationToken)
    {
        var profile = await profileRepository.GetAsync(cancellationToken);

        if (profile is null)
            return RedirectToAction("Logout", "Auth");

        return View(ProfileViewMOD.From(profile, tab));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditName(EditNameViewMOD editNameForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError(GetFirstModelError("Msg_NameInvalid"), tab: null);

        var result = await profileRepository.EditNameAsync(
            new EditNameMOD { Name = editNameForm.Name },
            cancellationToken);

        if (!result.IsSuccess)
            return RedirectWithError(localizer[result.ErrorKey!].Value, tab: null);

        await RefreshSignInAsync(result.Value!);
        return RedirectWithSuccess("Msg_NameUpdated", tab: null);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEmail(EditEmailViewMOD editEmailForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError(GetFirstModelError("Msg_EmailInvalid"), SecurityTab);

        var result = await profileRepository.EditEmailAsync(
            new EditEmailMOD { Email = editEmailForm.Email, Password = editEmailForm.Password },
            cancellationToken);

        if (!result.IsSuccess)
            return RedirectWithError(localizer[result.ErrorKey!].Value, SecurityTab);

        await RefreshSignInAsync(result.Value!);
        return RedirectWithSuccess("Msg_EmailUpdated", SecurityTab);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewMOD changePasswordForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
            return RedirectWithError(GetFirstModelError("Msg_DataInvalid"), SecurityTab);

        var result = await profileRepository.ChangePasswordAsync(new ChangePasswordMOD
        {
            CurrentPassword = changePasswordForm.CurrentPassword,
            NewPassword = changePasswordForm.NewPassword,
        }, cancellationToken);

        return result.IsSuccess
            ? RedirectWithSuccess("Msg_PasswordChanged", SecurityTab)
            : RedirectWithError(localizer[result.ErrorKey!].Value, SecurityTab);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SavePreference(string key, string value, CancellationToken cancellationToken)
    {
        var result = await profileRepository.SavePreferenceAsync(
            new PreferenceMOD { Key = key ?? string.Empty, Value = value ?? string.Empty },
            cancellationToken);

        if (!result.IsSuccess)
            return BadRequest();

        await RefreshSignInAsync(result.Value!);
        return Ok();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAccount(string? password, CancellationToken cancellationToken)
    {
        var result = await profileRepository.DeleteAccountAsync(
            new DeleteAccountMOD { Password = password },
            cancellationToken);

        if (!result.IsSuccess)
            return RedirectWithError(localizer[result.ErrorKey!].Value, AccountTab);

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "" });
    }

    private IActionResult RedirectWithSuccess(string messageKey, string? tab)
    {
        TempData["Success"] = localizer[messageKey].Value;
        return RedirectToIndex(tab);
    }

    private IActionResult RedirectWithError(string message, string? tab)
    {
        TempData["Error"] = message;
        return RedirectToIndex(tab);
    }

    private IActionResult RedirectToIndex(string? tab)
        => tab is null
            ? RedirectToAction("Index")
            : RedirectToAction("Index", new { tab });

    // A mensagem de validação já vem traduzida pelo DataAnnotationsLocalization,
    // então usa a primeira do ModelState e cai no fallback só quando não há nenhuma.
    private string GetFirstModelError(string fallbackKey)
        => ModelState.Values
            .SelectMany(entry => entry.Errors)
            .FirstOrDefault()?.ErrorMessage ?? localizer[fallbackKey].Value;
}
