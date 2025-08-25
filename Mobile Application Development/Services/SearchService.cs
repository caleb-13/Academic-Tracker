// Services/SearchService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;

namespace Mobile_Application_Development.Services
{
    // Lightweight DTO the UI can bind to
    public record SearchResult(string Entity, int Id, string Title, string Subtitle);

    public class SearchService
    {
        private readonly TermDatabase _db;

        public SearchService(TermDatabase db)
        {
            _db = db;
        }

        public async Task<List<SearchResult>> SearchAsync(string query)
        {
            query = (query ?? string.Empty).Trim();
            if (query.Length == 0) return new();

            var results = new List<SearchResult>();

            // --- Terms ---
            var terms = await _db.GetTermsAsync();
            foreach (var t in terms.Where(t => Contains(t.Title, query)))
            {
                var subtitle = $"{t.StartDate:d} – {t.EndDate:d}";
                results.Add(new SearchResult("Term", t.Id, t.Title, subtitle));
            }

            // --- Courses ---
            var courses = await _db.GetAllCoursesAsync();
            foreach (var c in courses.Where(c =>
                Contains($"{c.Title} {c.Status} {c.InstructorName} {c.InstructorEmail} {c.InstructorPhone}", query)))
            {
                var subtitle = $"{c.Status} • {c.StartDate:d} – {c.EndDate:d}";
                results.Add(new SearchResult("Course", c.Id, c.Title, subtitle));
            }

            // --- Assessments ---
            var assessments = await _db.GetAllAssessmentsAsync();
            foreach (var a in assessments.Where(a =>
                Contains($"{a.Title} {a.Type}", query)))
            {
                var subtitle = $"{a.Type} • Due {a.DueDate:g}";
                results.Add(new SearchResult("Assessment", a.Id, a.Title, subtitle));
            }

            return results
                .OrderBy(r => r.Entity)   // groups Terms/Courses/Assessments together
                .ThenBy(r => r.Title)
                .ToList();
        }

        private static bool Contains(string? source, string q) =>
            (source ?? string.Empty).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0;
    }
}
