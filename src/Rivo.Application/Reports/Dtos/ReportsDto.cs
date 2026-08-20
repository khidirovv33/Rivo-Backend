namespace Rivo.Application.Reports.Dtos;

/// <summary>Общая табличная форма для всех 8 отчётов (раздел 15 ТЗ) и их экспорта.</summary>
public class ReportTableDto
{
    public string Title { get; set; } = null!;

    public List<string> Columns { get; set; } = [];

    public List<List<string>> Rows { get; set; } = [];
}

public enum ReportExportFormat
{
    Pdf = 1,
    Excel = 2,
    Csv = 3,
}
