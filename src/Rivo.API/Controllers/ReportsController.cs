using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Rivo.API.Filters;
using Rivo.Application.Common.Models;
using Rivo.Application.Reports.Dtos;
using Rivo.Application.Reports.Interfaces;

namespace Rivo.API.Controllers;

[ApiController]
[Authorize]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly IReportsService _service;

    public ReportsController(IReportsService service)
    {
        _service = service;
    }

    [HttpGet("sales")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetSales(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetSalesReportAsync(from, to, ct)));

    [HttpGet("inventory")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetInventory(CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetInventoryReportAsync(ct)));

    [HttpGet("financial")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetFinancial(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetFinancialReportAsync(from, to, ct)));

    [HttpGet("profit")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetProfit(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetProfitReportAsync(from, to, ct)));

    [HttpGet("purchases")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetPurchases(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetPurchaseReportAsync(from, to, ct)));

    [HttpGet("employees")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetEmployees(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetEmployeeReportAsync(from, to, ct)));

    [HttpGet("audit")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetAudit(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetAuditReportAsync(from, to, ct)));

    [HttpGet("inventory-difference")]
    [PermissionAuthorize("Finance.Read")]
    public async Task<ActionResult<ApiResponse<ReportTableDto>>> GetInventoryDifference(
        [FromQuery] DateTime from, [FromQuery] DateTime to, CancellationToken ct) =>
        Ok(ApiResponse<ReportTableDto>.Ok(await _service.GetInventoryDifferenceReportAsync(from, to, ct)));

    /// <summary>
    /// Универсальный экспорт: сначала получаете отчёт через один из GET выше, затем POST'ите его же
    /// сюда с нужным форматом. Разделено на два шага, чтобы фронт мог сперва показать таблицу,
    /// а потом (по клику "Экспорт") получить файл без повторного построения отчёта на клиенте.
    /// </summary>
    [HttpPost("export/{format}")]
    [PermissionAuthorize("Finance.Read")]
    public IActionResult Export(ReportExportFormat format, [FromBody] ReportTableDto report)
    {
        var bytes = _service.Export(report, format);
        var (contentType, extension) = format switch
        {
            ReportExportFormat.Pdf => ("application/pdf", "pdf"),
            ReportExportFormat.Excel => ("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "xlsx"),
            ReportExportFormat.Csv => ("text/csv", "csv"),
            _ => ("application/octet-stream", "bin"),
        };

        return File(bytes, contentType, $"{report.Title}.{extension}");
    }
}
