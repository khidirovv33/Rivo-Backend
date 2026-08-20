using System.Text;
using Rivo.Application.Common.Interfaces;

namespace Rivo.Infrastructure.ExternalServices;

public class CsvExportService : ICsvExportService
{
    public byte[] Export(IReadOnlyList<string> columns, IReadOnlyList<IReadOnlyList<string>> rows)
    {
        var builder = new StringBuilder();
        builder.AppendLine(string.Join(',', columns.Select(Escape)));

        foreach (var row in rows)
        {
            builder.AppendLine(string.Join(',', row.Select(Escape)));
        }

        return new UTF8Encoding(true).GetBytes(builder.ToString());
    }

    private static string Escape(string value)
    {
        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
