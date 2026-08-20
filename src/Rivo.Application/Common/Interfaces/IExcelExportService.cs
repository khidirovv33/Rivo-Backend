namespace Rivo.Application.Common.Interfaces;

public interface IExcelExportService
{
    byte[] Export(string sheetTitle, IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows);
}
