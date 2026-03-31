using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services.Interfaces;

public interface IWelcomeService
{
    Task<WelcomeViewModel?> GetWelcomeViewModelAsync(Guid userId, int step);
    Task<Result<User>> CompleteOnboardingAsync(Guid userId);
}
