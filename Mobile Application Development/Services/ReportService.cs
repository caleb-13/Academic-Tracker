
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;

namespace Mobile_Application_Development.Services
{
    public class ReportService
    {
        private readonly TermDatabase _db;

        public ReportService() : this(new TermDatabase()) { }
        public ReportService(TermDatabase db) { _db = db; }

        public async Task<string> ExportUpcomingAssessmentsCsvAsync(DateTime fromInclusive, DateTime toInclusive)
        {
            var assessments = await _db.GetAllAssessmentsAsync();
            var courses = await _db.GetAllCoursesAsync();
            var terms = await _db.GetTermsAsync();

            var filtered = assessments
                .Where(a => a.DueDate.Date >= fromInclusive.Date && a.DueDate.Date <= toInclusive.Date)
                .OrderBy(a => a.DueDate)
                .Select(a =>
                {
                    var course = courses.FirstOrDefault(c => c.Id == a.CourseId);
                    var term = (course != null) ? terms.FirstOrDefault(t => t.Id == course.TermId) : null;

                    return new
                    {
                        AssessmentTitle = a.Title,
                        Type = a.Type.ToString(),
                        DueDate = a.DueDate,                
                        CourseTitle = course?.Title ?? "",
                        CourseStatus = course?.Status.ToString() ?? "",
                        TermName = term?.Title ?? "",
                        TermStart = term?.StartDate,
                        TermEnd = term?.EndDate
                    };
                })
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine("Report Title,Upcoming Assessments");
            sb.AppendLine($"Generated (UTC),{DateTime.UtcNow:o}");
            sb.AppendLine("Assessment Title,Type,Due Date (Local),Course,Course Status,Term,Term Start,Term End");

            foreach (var r in filtered)
            {
                var dueLocal = r.DueDate.ToLocalTime().ToString("g");
                var termStart = r.TermStart?.ToString("d") ?? "";
                var termEnd = r.TermEnd?.ToString("d") ?? "";
                sb.AppendLine($"{Escape(r.AssessmentTitle)},{r.Type},{Escape(dueLocal)},{Escape(r.CourseTitle)},{r.CourseStatus},{Escape(r.TermName)},{termStart},{termEnd}");
            }

            
            var fileName = $"Upcoming_Assessments_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
            var path = Path.Combine(FileSystem.CacheDirectory, fileName);
            File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
            return path;
        }

        private static string Escape(string? s)
        {
            s ??= "";
            return (s.Contains(',') || s.Contains('"'))
                ? $"\"{s.Replace("\"", "\"\"")}\""
                : s;
        }
    }
}
