using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services.Interfaces;

public interface IQuotesService
{
    Task<(List<QuoteViewModel> Quotes, string? ErrorTechDetails)> GetQuotesAsync();
}
