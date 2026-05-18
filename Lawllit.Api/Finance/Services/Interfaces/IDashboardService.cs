using Lawllit.Models.Finance.ViewModels;

namespace Lawllit.Api.Finance.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildDashboardAsync(Guid userId, int? requestedMonth, int? requestedYear);
}
