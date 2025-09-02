using System.Linq;               // needed for OrderBy
using System.Text;
using Mobile_Application_Development.Models;

namespace Mobile_Application_Development.Services
{
    public static class ReportFormatter
    {
        public static string BuildCsv(IEnumerable<Assessment> assessments, DateTime from, DateTime to, string title = "Upcoming Assessments Report")
        {
            var sb = new StringBuilder();
            sb.AppendLine(title);
            sb.AppendLine($"Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
            sb.AppendLine($"Range: {from:yyyy-MM-dd} to {to:yyyy-MM-dd}");
            sb.AppendLine("AssessmentId,Type,Title,StartDate,DueDate");

            foreach (var a in assessments.OrderBy(a => a.DueDate))
            {
                var type = a.Type.ToString();
                var safeTitle = (a.Title ?? string.Empty).Replace("\"", "\"\"");
                sb.AppendLine($"{a.Id},{type},\"{safeTitle}\",{a.StartDate:yyyy-MM-dd},{a.DueDate:yyyy-MM-dd}");
            }
            return sb.ToString();
        }
    }
}
