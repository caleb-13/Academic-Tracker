using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.Generic;
using Microsoft.Maui.Controls;
using Mobile_Application_Development.Services;

namespace Mobile_Application_Development.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private readonly ReportService _reportService = new ReportService();

        private DateTime _fromDate = DateTime.Today.AddDays(-7);
        public DateTime FromDate
        {
            get => _fromDate; set { _fromDate = value; OnPropertyChanged(); }
        }

        private DateTime _toDate = DateTime.Today.AddDays(30);
        public DateTime ToDate
        {
            get => _toDate; set { _toDate = value; OnPropertyChanged(); }
        }

        private string _lastReportPath = string.Empty;
        public string LastReportPath
        {
            get => _lastReportPath; set { _lastReportPath = value; OnPropertyChanged(); }
        }

        public ICommand GenerateCommand { get; }

        public ReportsViewModel()
        {
            Title = "Reports";
            GenerateCommand = new Command(async () => await GenerateAsync());
        }

       
        private async Task GenerateAsync()
        {
            if (ToDate < FromDate)
            {
                await Application.Current!.MainPage!.DisplayAlert("Invalid Range", "End date must be after start date.", "OK");
                return;
            }

            try
            {
                IsBusy = true;

                
                var path = await _reportService.ExportUpcomingAssessmentsCsvAsync(FromDate, ToDate);
                LastReportPath = path;

                
                var csv = File.ReadAllText(path);
                var html = CsvToHtml(csv, out var title, out _);

                var previewPage = new ContentPage
                {
                    Title = title,
                    Content = new WebView
                    {
                        Source = new HtmlWebViewSource { Html = html }
                    },
                    Padding = 0
                };

                await Application.Current!.MainPage!.Navigation.PushModalAsync(previewPage);
            }
            catch (Exception ex)
            {
                await Application.Current!.MainPage!.DisplayAlert("Report Error", ex.Message, "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

      

        private static string CsvToHtml(string csv, out string title, out string generatedIso)
        {
            var lines = csv.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);

           
            title = lines.Length > 0 ? GetCell(lines[0], 1) : "Report";
            generatedIso = lines.Length > 1 ? GetCell(lines[1], 1) : DateTime.UtcNow.ToString("o");

            
            var generatedNice = FormatUtcIsoNice(generatedIso);

            var sb = new StringBuilder();
            sb.Append("""
                    <!DOCTYPE html>
                    <html><head><meta charset="utf-8">
                    <style>
                    body{font-family:system-ui,-apple-system,Segoe UI,Roboto,Arial,sans-serif;margin:12px;}
                    h2{margin:0 0 6px 0;}
                    small{color:#666;}
                    table{border-collapse:collapse;width:100%;margin-top:12px;}
                    th,td{border:1px solid #ddd;padding:6px 8px;font-size:14px;}
                    th{background:#f3f3f3;text-align:left;}
                    tr:nth-child(even){background:#fafafa;}
                    </style></head><body>
                    """);

            sb.Append($"<h2>{HtmlEncode(title)}</h2>");
            sb.Append($"<small>Generated: {HtmlEncode(generatedNice)}</small>");

            sb.Append("<table><thead><tr>");
            if (lines.Length > 2)
            {
                foreach (var h in SplitCsvLine(lines[2]))
                    sb.Append($"<th>{HtmlEncode(h)}</th>");
                sb.Append("</tr></thead><tbody>");

                for (int i = 3; i < lines.Length; i++)
                {
                    var cells = SplitCsvLine(lines[i]);
                    if (cells.Count == 0) continue;
                    sb.Append("<tr>");
                    foreach (var c in cells)
                        sb.Append($"<td>{HtmlEncode(c)}</td>");
                    sb.Append("</tr>");
                }
                sb.Append("</tbody></table>");
            }
            else
            {
                sb.Append("<p>No rows.</p>");
            }

            sb.Append("</body></html>");
            return sb.ToString();
        }

        private static string HtmlEncode(string? s) =>
            (s ?? string.Empty).Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private static string GetCell(string line, int index)
        {
            var cells = SplitCsvLine(line);
            return (index >= 0 && index < cells.Count) ? cells[index] : string.Empty;
        }

        
        private static List<string> SplitCsvLine(string? line)
        {
            var result = new List<string>();
            if (line == null) return result;

            var sb = new StringBuilder();
            bool inQuotes = false;

            for (int i = 0; i < line.Length; i++)
            {
                char c = line[i];
                if (inQuotes)
                {
                    if (c == '"')
                    {
                        if (i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                        else { inQuotes = false; }
                    }
                    else sb.Append(c);
                }
                else
                {
                    if (c == ',')
                    {
                        result.Add(sb.ToString());
                        sb.Clear();
                    }
                    else if (c == '"')
                    {
                        inQuotes = true;
                    }
                    else sb.Append(c);
                }
            }
            result.Add(sb.ToString());
            return result;
        }

       
        private static string FormatUtcIsoNice(string iso)
        {
            if (DateTime.TryParse(iso, null, System.Globalization.DateTimeStyles.AdjustToUniversal | System.Globalization.DateTimeStyles.AssumeUniversal, out var dt))
            {
                return dt.ToUniversalTime().ToString("MMM d, yyyy h:mm tt 'UTC'");
            }
            return iso; 
        }
    }
}