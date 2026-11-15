using System.Globalization;
using System.Text;
using Pos.Application;
namespace Pos.Infrastructure;

public sealed class CsvResultExporter : IResultExporter
{
    public void Export(IReadOnlyList<ResultRow> rows, string path)
    {
        var fullPath = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var lines = new List<string> { "student_id,name,score,evaluated,total,policy" };
        lines.AddRange(rows.Select(r => string.Join(",", Escape(r.StudentId), Escape(r.Name),
            r.Score.ToString(CultureInfo.InvariantCulture), r.Evaluated, r.Total, Escape(r.Policy))));
        File.WriteAllLines(fullPath, lines, new UTF8Encoding(true));
    }
    private static string Escape(string text) => "\"" + text.Replace("\"", "\"\"") + "\"";
}
