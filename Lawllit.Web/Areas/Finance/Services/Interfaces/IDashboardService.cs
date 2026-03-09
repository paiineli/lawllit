using Lawllit.Web.Areas.Finance.Models;

namespace Lawllit.Web.Areas.Finance.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardViewModel> BuildDashboardAsync(Guid userId, int? requestedMonth, int? requestedYear);
}
