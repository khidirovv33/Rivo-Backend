namespace Rivo.Application.Common.Interfaces;

/// <summary>Generic tabular export shared by all Reports (§15 ТЗ: экспорт PDF, Excel, CSV).</summary>
public interface ICsvExportService
{
    byte[] Export(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows);
}
