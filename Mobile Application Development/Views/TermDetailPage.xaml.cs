using System.Collections.ObjectModel;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;

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

       
        private async void OnSaveTermClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Term.Title))
            {
                await DisplayAlert("Validation", "Please enter a term title.", "OK");
                return;
            }
            if (Term.EndDate < Term.StartDate)
            {
                await DisplayAlert("Validation", "End date cannot be before start date.", "OK");
                return;
            }

            try
            {
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