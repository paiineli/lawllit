using Lawllit.Web.Areas.Finance.Models;
using Lawllit.Web.Areas.Finance.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
public class AuthController(IAuthService authService, IEmailService emailService, IStringLocalizer<SharedResource> localizer) : BaseController
{
    [HttpGet]
    public IActionResult Login()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Login(LoginViewModel loginForm)
    {
        if (!ModelState.IsValid) return View(loginForm);

        var result = await authService.ValidateLoginAsync(loginForm.Email, loginForm.Password);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, localizer[result.ErrorKey!]);
            return View(loginForm);
        }

        await SignInAsync(result.Value!);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Dashboard");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> Register(RegisterViewModel registerForm)
    {
        if (!ModelState.IsValid) return View(registerForm);

        var language = CultureInfo.CurrentUICulture.Name;
        var result = await authService.RegisterAsync(registerForm.Name, registerForm.Email, registerForm.Password, language);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError(nameof(registerForm.Email), localizer[result.ErrorKey!]);
            return View(registerForm);
        }

        var confirmUrl = Url.Action("ConfirmEmail", "Auth", new { token = result.Value!.EmailConfirmationToken }, Request.Scheme)!;
        await emailService.SendConfirmationEmailAsync(result.Value.Email, result.Value.Name, confirmUrl, language);

        TempData["Success"] = localizer["Msg_EmailConfirmSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token)
    {
        var result = await authService.ConfirmEmailAsync(token);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Login");
        }

        await SignInAsync(result.Value!);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home", new { area = "Finance" });
    }

    [HttpGet]
    public IActionResult ForgotPassword() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel forgotPasswordForm)
    {
        if (!ModelState.IsValid) return View(forgotPasswordForm);

        var passwordResetResult = await authService.BeginPasswordResetAsync(forgotPasswordForm.Email.Trim().ToLower());
        if (passwordResetResult.IsSuccess)
        {
            var resetUrl = Url.Action("ResetPassword", "Auth", new { token = passwordResetResult.Value!.Token }, Request.Scheme)!;
            var language = CultureInfo.CurrentUICulture.Name;
            await emailService.SendPasswordResetEmailAsync(passwordResetResult.Value.User.Email, passwordResetResult.Value.User.Name, resetUrl, language);
        }

        TempData["Success"] = localizer["Msg_ForgotEmailSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token)
    {
        if (!await authService.ValidatePasswordResetTokenAsync(token))
        {
            TempData["Error"] = localizer["Msg_InvalidLink"].Value;
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewModel { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel resetPasswordForm)
    {
        if (!ModelState.IsValid) return View(resetPasswordForm);

        var result = await authService.ResetPasswordAsync(resetPasswordForm.Token, resetPasswordForm.Password);
        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Login");
        }

        TempData["Success"] = localizer["Msg_PasswordReset"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties { RedirectUri = Url.Action("GoogleCallback") };
        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet]
    public async Task<IActionResult> GoogleCallback()
    {
        var authResult = await HttpContext.AuthenticateAsync("External");
        if (!authResult.Succeeded) return RedirectToAction("Login");

        var googleId = authResult.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = authResult.Principal!.FindFirstValue(ClaimTypes.Email);
        if (googleId is null || email is null) return RedirectToAction("Login");

        var name = StringHelpers.ToTitleCase(authResult.Principal!.FindFirstValue(ClaimTypes.Name) ?? email);
        var language = CultureInfo.CurrentUICulture.Name;
        var user = await authService.GetOrCreateGoogleUserAsync(googleId, email, name, language);

        await HttpContext.SignOutAsync("External");
        await SignInAsync(user);
        return RedirectToAction("Index", "Dashboard");
    }
}
