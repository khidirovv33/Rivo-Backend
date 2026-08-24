using Rivo.Application.Dashboard.Dtos;

namespace Rivo.Application.Dashboard.Interfaces;

public interface IDashboardService
{
    Task<DashboardOverviewDto> GetOverviewAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);

    /// <summary>Сводка "сегодня + неделя" для главного экрана (Обзор) — не путать с финансовым GetOverviewAsync.</summary>
    Task<DashboardDto> GetHomeOverviewAsync(Guid tenantId, Guid? branchId, CancellationToken cancellationToken = default);
}
