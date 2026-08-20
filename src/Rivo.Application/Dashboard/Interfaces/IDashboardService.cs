using Rivo.Application.Dashboard.Dtos;

namespace Rivo.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
