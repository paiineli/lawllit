using Lawllit.Api.Finance.Repositories;
using Lawllit.Model.Common;
using Lawllit.Model.Finance;
using Lawllit.Model.Finance.Contracts;

namespace Lawllit.Api.Finance.Services;

public sealed class WelcomeService(IUserREP userRepository) : IWelcomeService
{
    private const int FirstStep = 1;
    private const int LastStep = 4;

    public async Task<WelcomeMOD?> GetAsync(Guid userId, int step, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null) return null;

        return new WelcomeMOD
        {
            CurrentStep = Math.Clamp(step, FirstStep, LastStep),
            UserName = user.Name,
            Language = user.Language,
            Currency = user.Currency,
            Theme = user.Theme,
            FontSize = user.FontSize,
        };
    }

    public async Task<Result<UserMOD>> CompleteAsync(Guid userId, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserMOD>.Failure("Msg_OperationNotAllowed");

        user.IsOnboardingCompleted = true;
        await userRepository.UpdateAsync(user, cancellationToken);

        return Result<UserMOD>.Success(user);
    }
}

#region Interfaces

public interface IWelcomeService
{
    Task<WelcomeMOD?> GetAsync(Guid userId, int step, CancellationToken cancellationToken);
    Task<Result<UserMOD>> CompleteAsync(Guid userId, CancellationToken cancellationToken);
}

#endregion
