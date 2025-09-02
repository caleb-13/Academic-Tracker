using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Mobile_Application_Development.Views
{
    public partial class TermDetailPage : ContentPage
    {
        private const int MaxCoursesPerTerm = 6;

        private readonly TermDatabase _db;

        public Term Term { get; }
        public ObservableCollection<Course> Courses { get; } = new();

        public TermDetailPage(TermDatabase db, Term term)
        {
            InitializeComponent();

            _db = db ?? throw new ArgumentNullException(nameof(db));
            Term = term ?? throw new ArgumentNullException(nameof(term));

            
            BindingContext = this;
           }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await LoadCoursesAsync();
        }

        private async Task LoadCoursesAsync()
        {
            try
            {
                Courses.Clear();

                if (Term.Id != 0)
                {
                    var list = await _db.GetCoursesForTermAsync(Term.Id);
                    foreach (var c in list)
                        Courses.Add(c);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not load courses:\n{ex.Message}", "OK");
            }
            finally
            {
                UpdateAddCourseButtonState();
            }
        }

        private void UpdateAddCourseButtonState()
        {
            if (AddCourseButton != null)
                AddCourseButton.IsEnabled = Courses.Count < MaxCoursesPerTerm;
        }

        private static string Clean(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var trimmed = s.Trim();
            return Regex.Replace(trimmed, @"\s{2,}", " ");
        }

        private async void OnSaveTermClicked(object sender, EventArgs e)
        {
            var errors = new List<string>();

           
            var titleClean = Clean(Term.Title);
            var start = Term.StartDate;
            var end = Term.EndDate;

            if (string.IsNullOrWhiteSpace(titleClean))
                errors.Add("Please enter a term title.");

            if (end < start)
                errors.Add("End date cannot be before start date.");

            
            if (Term.Id != 0)
            {
                try
                {
                    var courses = await _db.GetCoursesForTermAsync(Term.Id);
                    foreach (var c in courses)
                    {
                        if (c.StartDate < start || c.EndDate > end)
                        {
                            errors.Add($"Course '{c.Title}' falls outside the new term dates.");
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    errors.Add($"Could not verify courses in term: {ex.Message}");
                }
            }

            if (errors.Count > 0)
            {
                await DisplayAlert("Please fix the following", "• " + string.Join("\n• ", errors), "OK");
                return;
            }

            try
            {
                
                Term.Title = titleClean;
                Term.StartDate = start;
                Term.EndDate = end;

                await _db.SaveTermAsync(Term);
                await DisplayAlert("Saved", "Term saved.", "OK");
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not save term:\n{ex.Message}", "OK");
            }
        }

        private async void OnDeleteTermClicked(object sender, EventArgs e)
        {
            if (Term.Id == 0)
            {
                await Navigation.PopAsync();
                return;
            }

            var confirm = await DisplayAlert("Delete", "Delete this term and all its courses?", "Yes", "No");
            if (!confirm) return;

            try
            {
                await _db.DeleteTermAsync(Term);
                await Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not delete term:\n{ex.Message}", "OK");
            }
        }

        private async void OnAddCourseClicked(object sender, EventArgs e)
        {
            try
            {
                
                var dbCount = Term.Id == 0 ? 0 : await _db.GetCourseCountForTermAsync(Term.Id);
                if (dbCount >= MaxCoursesPerTerm || Courses.Count >= MaxCoursesPerTerm)
                {
                    await DisplayAlert("Limit", $"You can only add {MaxCoursesPerTerm} courses per term.", "OK");
                    return;
                }

               
                if (Term.Id == 0)
                    await _db.SaveTermAsync(Term);

                var course = new Course
                {
                    TermId = Term.Id,
                    Title = string.Empty,
                    StartDate = DateTime.Today,
                    EndDate = DateTime.Today.AddDays(1)
                };

                await Navigation.PushAsync(new CourseDetailPage(_db, course));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open Add Course:\n{ex.Message}", "OK");
            }
        }

        private async void OnRemoveCourseClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not Course course)
                return;

            var confirm = await DisplayAlert("Remove Course", $"Remove '{course.Title}'?", "Yes", "No");
            if (!confirm) return;

            try
            {
                await _db.DeleteCourseAsync(course);
                Courses.Remove(course);
                UpdateAddCourseButtonState();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not remove course:\n{ex.Message}", "OK");
            }
        }

        private async void OnEditCourseClicked(object sender, EventArgs e)
        {
            if (sender is not Button btn || btn.CommandParameter is not Course course)
                return;

            try
            {
                await Navigation.PushAsync(new CourseDetailPage(_db, course));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Could not open Course Details:\n{ex.Message}", "OK");
            }
        }
    }
}
