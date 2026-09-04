using Lawllit.Repository.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Lawllit.Site.Areas.Finance.Controllers;

[Area("Finance")]
[Authorize]
public class WelcomeController(IWelcomeREP welcomeRepository) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> Index(int step, CancellationToken cancellationToken)
    {
        if (User.FindFirstValue("is_onboarding_completed") == "true")
            return RedirectToAction("Index", "Dashboard");

        var welcome = await welcomeRepository.GetAsync(step <= 0 ? 1 : step, cancellationToken);

        if (welcome is null)
            return RedirectToAction("Logout", "Auth");

        return View(welcome);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(CancellationToken cancellationToken)
    {
        var result = await welcomeRepository.CompleteAsync(cancellationToken);

        if (result.IsSuccess)
            await RefreshSignInAsync(result.Value!);

        return RedirectToAction("Index", "Dashboard");
    }
}
