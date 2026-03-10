using Lawllit.Data.Finance.Entities;
using Lawllit.Web.Areas.Finance.Models;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface IWelcomeService
{
    Task<WelcomeViewModel?> GetWelcomeViewModelAsync(Guid userId, int step);
    Task<Result<User>> CompleteOnboardingAsync(Guid userId);
}
