using Lawllit.Api.Finance.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Web.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class WelcomeController(IWelcomeService welcomeService) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(int step = 1)
    {
        if (User.FindFirstValue("is_onboarding_completed") == "true")
            return RedirectToAction("Index", "Dashboard");

        var viewModel = await welcomeService.GetWelcomeViewModelAsync(GetUserId(), step);
        if (viewModel is null)
            return RedirectToAction("Logout", "Auth");

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete()
    {
        var result = await welcomeService.CompleteOnboardingAsync(GetUserId());
        if (!result.IsSuccess)
            return RedirectToAction("Index", "Dashboard");

        await SignInAsync(result.Value!);
        return RedirectToAction("Index", "Dashboard");
    }
}
