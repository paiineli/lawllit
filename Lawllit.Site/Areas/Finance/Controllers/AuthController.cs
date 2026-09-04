using Lawllit.Model.Common;
using Lawllit.Model.Finance.Contracts;
using Lawllit.Repository.Finance;
using Lawllit.Site.Models.Finance;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.Security.Claims;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
public class AuthController(IAuthREP authRepository, IStringLocalizer<SharedResource> localizer) : BaseController
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
    public async Task<IActionResult> Login(LoginViewMOD loginForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(loginForm);

        var result = await authRepository.LoginAsync(
            new LoginMOD { Email = loginForm.Email, Password = loginForm.Password },
            cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(string.Empty, localizer[result.ErrorKey!].Value);
            return View(loginForm);
        }

        return await SignInAndRedirectAsync(result.Value!);
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
    public async Task<IActionResult> Register(RegisterViewMOD registerForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(registerForm);

        var result = await authRepository.RegisterAsync(new RegisterMOD
        {
            Name = registerForm.Name,
            Email = registerForm.Email,
            Password = registerForm.Password,
            Language = CultureInfo.CurrentUICulture.Name,
            ConfirmationUrlTemplate = BuildAbsoluteUrl("ConfirmEmail"),
        }, cancellationToken);

        if (!result.IsSuccess)
        {
            ModelState.AddModelError(nameof(registerForm.Email), localizer[result.ErrorKey!].Value);
            return View(registerForm);
        }

        TempData["Success"] = localizer["Msg_EmailConfirmSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ConfirmEmail(string token, CancellationToken cancellationToken)
    {
        var result = await authRepository.ConfirmEmailAsync(token, cancellationToken);

        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Login");
        }

        return await SignInAndRedirectAsync(result.Value!);
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
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewMOD forgotPasswordForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(forgotPasswordForm);

        // A resposta é ignorada de propósito. Conta inexistente e conta existente mostram
        // a mesma mensagem, para a tela não servir de consulta de quem tem cadastro.
        await authRepository.ForgotPasswordAsync(new ForgotPasswordMOD
        {
            Email = forgotPasswordForm.Email,
            ResetUrlTemplate = BuildAbsoluteUrl("ResetPassword"),
        }, cancellationToken);

        TempData["Success"] = localizer["Msg_ForgotEmailSent"].Value;
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string token, CancellationToken cancellationToken)
    {
        if (!await authRepository.IsPasswordResetTokenValidAsync(token, cancellationToken))
        {
            TempData["Error"] = localizer["Msg_InvalidLink"].Value;
            return RedirectToAction("Login");
        }

        return View(new ResetPasswordViewMOD { Token = token });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewMOD resetPasswordForm, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(resetPasswordForm);

        var result = await authRepository.ResetPasswordAsync(
            new ResetPasswordMOD { Token = resetPasswordForm.Token, NewPassword = resetPasswordForm.Password },
            cancellationToken);

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
    public async Task<IActionResult> GoogleCallback(CancellationToken cancellationToken)
    {
        var externalAuth = await HttpContext.AuthenticateAsync("External");
        if (!externalAuth.Succeeded) return RedirectToAction("Login");

        var googleId = externalAuth.Principal!.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = externalAuth.Principal!.FindFirstValue(ClaimTypes.Email);
        if (googleId is null || email is null) return RedirectToAction("Login");

        var result = await authRepository.LoginWithGoogleAsync(new GoogleUserMOD
        {
            GoogleId = googleId,
            Email = email,
            Name = externalAuth.Principal!.FindFirstValue(ClaimTypes.Name) ?? email,
            Language = CultureInfo.CurrentUICulture.Name,
        }, cancellationToken);

        await HttpContext.SignOutAsync("External");

        if (!result.IsSuccess)
        {
            TempData["Error"] = localizer[result.ErrorKey!].Value;
            return RedirectToAction("Login");
        }

        return await SignInAndRedirectAsync(result.Value!);
    }

    private async Task<IActionResult> SignInAndRedirectAsync(AuthResultMOD authResult)
    {
        await SignInAsync(authResult.User, authResult.Token);

        return authResult.User.IsOnboardingCompleted
            ? RedirectToAction("Index", "Dashboard")
            : RedirectToAction("Index", "Welcome");
    }

    // A API precisa da URL completa para montar o link do e-mail, e só o Site
    // conhece as próprias rotas. O {token} é substituído do outro lado.
    private string BuildAbsoluteUrl(string action)
        => Url.Action(action, "Auth", new { token = "__TOKEN__" }, Request.Scheme)!
            .Replace("__TOKEN__", "{token}");
}
