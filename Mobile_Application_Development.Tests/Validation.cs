using System.Text.RegularExpressions;

namespace Mobile_Application_Development.Utility
{
    public static class Validation
    {
        public static string Clean(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var trimmed = s.Trim();
            return Regex.Replace(trimmed, @"\s{2,}", " ");
        }

        public static bool IsValidEmail(string? email) =>
            !string.IsNullOrWhiteSpace(email) &&
            Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant);

        public static bool IsLikelyPhone(string? phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length >= 10 && digits.Length <= 15;
        }
    }
}
