// Services/ValidationService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mobile_Application_Development.Models;

namespace Mobile_Application_Development.Services
{
    public static class ValidationService
    {
        // Simple result that returns a sanitized copy + error list
        public sealed class ValidationResult<T>
        {
            public bool IsValid => Errors.Count == 0;
            public T? Value { get; }
            public List<string> Errors { get; } = new();
            public ValidationResult(T? value) { Value = value; }
        }

        // ---------- Common helpers ----------
        private static string Clean(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            // Trim and collapse internal whitespace
            var trimmed = s.Trim();
            return Regex.Replace(trimmed, @"\s{2,}", " ");
        }

        private static bool LooksLikeEmail(string? s) =>
            !string.IsNullOrEmpty(s) &&
            Regex.IsMatch(s, @"^[A-Za-z0-9._%+\-]+@[A-Za-z0-9.\-]+\.[A-Za-z]{2,}$");

        private static bool LooksLikePhone(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var digits = new string(s.Where(char.IsDigit).ToArray());
            return digits.Length >= 10 && digits.Length <= 15;
        }

        // ---------- Term ----------
        public static ValidationResult<Term> ValidateTerm(Term input)
        {
            // Make a sanitized copy (don’t mutate the original yet)
            var t = new Term
            {
                Id = input.Id,
                Title = Clean(input.Title),
                StartDate = input.StartDate,
                EndDate = input.EndDate
            };

            var result = new ValidationResult<Term>(t);

            if (string.IsNullOrWhiteSpace(t.Title))
                result.Errors.Add("Term title is required.");

            if (t.EndDate <= t.StartDate)
                result.Errors.Add("Term end date must be after start date.");

            return result;
        }

        public static ValidationResult<Course> ValidateCourse(Course input)
        {
            var c = new Course
            {
                Id = input.Id,
                TermId = input.TermId,
                Title = Clean(input.Title),
                StartDate = input.StartDate,
                EndDate = input.EndDate,
                Status = input.Status,
                InstructorName = Clean(input.InstructorName),
                InstructorEmail = Clean(input.InstructorEmail),
                InstructorPhone = Clean(input.InstructorPhone)
            };

            var result = new ValidationResult<Course>(c);

            if (string.IsNullOrWhiteSpace(c.Title))
                result.Errors.Add("Course title is required.");

            if (c.EndDate <= c.StartDate)
                result.Errors.Add("Course end date must be after start date.");

            if (!string.IsNullOrWhiteSpace(c.InstructorEmail) && !LooksLikeEmail(c.InstructorEmail))
                result.Errors.Add("Instructor email must look like name@domain.tld.");

            if (!string.IsNullOrWhiteSpace(c.InstructorPhone) && !LooksLikePhone(c.InstructorPhone))
                result.Errors.Add("Instructor phone must contain 10–15 digits.");

            return result;
        }

        public static ValidationResult<Assessment> ValidateAssessment(Assessment input)
        {
            var a = new Assessment
            {
                Id = input.Id,
                CourseId = input.CourseId,
                Title = Clean(input.Title),
                Type = input.Type,
                DueDate = input.DueDate
            };

            var result = new ValidationResult<Assessment>(a);

            if (string.IsNullOrWhiteSpace(a.Title))
                result.Errors.Add("Assessment title is required.");

            if (a.DueDate == default)
                result.Errors.Add("Assessment due date is required.");

            return result;
        }
    }
}
