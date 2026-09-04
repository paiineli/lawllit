using Microsoft.AspNetCore.Localization;

namespace Lawllit.Site.Common;

// O idioma escolhido pelo usuário viaja na claim, então vale mais que o cookie
// de cultura e mais que o Accept-Language do navegador.
public sealed class ClaimCultureProvider : RequestCultureProvider
{
    public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
    {
        var language = httpContext.User.FindFirst("language")?.Value;

        if (language is null) return NullProviderCultureResult;

        return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture: language, uiCulture: language));
    }
}
