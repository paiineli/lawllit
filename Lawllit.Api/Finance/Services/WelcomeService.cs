using Lawllit.Api.Finance.Repositories.Interfaces;
using Lawllit.Api.Finance.Services.Interfaces;
using Lawllit.Models.Finance;
using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services;

public class WelcomeService(IUserRepository userRepository) : IWelcomeService
{
    public async Task<WelcomeViewModel?> GetWelcomeViewModelAsync(Guid userId, int step)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return null;

        return new WelcomeViewModel
        {
            CurrentStep = Math.Clamp(step, 1, 4),
            UserName = user.Name,
            Language = user.Language,
            Currency = user.Currency,
            Theme = user.Theme,
            FontSize = user.FontSize,
        };
    }

    public async Task<Result<User>> CompleteOnboardingAsync(Guid userId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        if (user is null)
            return Result<User>.Failure("Msg_OperationNotAllowed");

        user.IsOnboardingCompleted = true;
        await userRepository.UpdateAsync(user);
        return Result<User>.Success(user);
    }
}
