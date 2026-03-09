using Lawllit.Web.Areas.Finance.Models;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface IQuotesService
{
    Task<(List<QuoteViewModel> Quotes, string? ErrorTechDetails)> GetQuotesAsync();
}
