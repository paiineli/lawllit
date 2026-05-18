using Microsoft.AspNetCore.Localization;

namespace Lawllit.Web;

public class ClaimCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var language = httpContext.User.FindFirst("language")?.Value;
        if (language is null) return NullProviderCultureResult;
        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture: language, uiCulture: language));
    }
}
