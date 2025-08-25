using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;
using System.Collections.Generic;
using Plugin.LocalNotification;
using Microsoft.Maui.ApplicationModel.DataTransfer;

namespace Mobile_Application_Development.ViewModels
{
    public class CourseDetailViewModel : ObservableObject
    {
        private readonly TermDatabase _db;
        private readonly Course _course;

        private Assessment? _perf;
        private Assessment? _obj;

        public CourseDetailViewModel(TermDatabase db, Course course)
        {
            _db = db;
            _course = course;

            Title = _course.Title ?? string.Empty;
            StartDate = _course.StartDate == default ? DateTime.Today : _course.StartDate;
            EndDate = _course.EndDate == default ? DateTime.Today.AddDays(1) : _course.EndDate;

            SelectedStatus = ToStatusString(_course.Status);
            InstructorName = _course.InstructorName ?? string.Empty;
            InstructorPhone = _course.InstructorPhone ?? string.Empty;
            InstructorEmail = _course.InstructorEmail ?? string.Empty;

            Notes = _course.Notes ?? string.Empty;
            StartAlertEnabled = _course.StartAlertEnabled;
            EndAlertEnabled = _course.EndAlertEnabled;

            _ = LoadAssessmentsAsync();

            SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsBusy);
            ShareNotesCommand = new RelayCommand(ShareNotes);

            DeletePerfCommand = new AsyncRelayCommand(DeletePerfAsync);
            DeleteObjCommand = new AsyncRelayCommand(DeleteObjAsync);
        }

        private async Task LoadAssessmentsAsync()
        {
            if (_course.Id == 0)
            {
                PerfTitle = string.Empty;
                PerfStartDate = StartDate;
                PerfEndDate = EndDate;
                ObjTitle = string.Empty;
                ObjStartDate = StartDate;
                ObjEndDate = EndDate;
                return;
            }

            var list = await _db.GetAssessmentsForCourseAsync(_course.Id);
            _perf = list.Find(a => a.Type == AssessmentType.Performance);
            _obj = list.Find(a => a.Type == AssessmentType.Objective);

            PerfTitle = _perf?.Title ?? string.Empty;
            PerfStartDate = _perf?.StartDate ?? StartDate;
            PerfEndDate = _perf?.DueDate ?? EndDate;
            PerfStartAlertEnabled = _perf?.StartAlertEnabled ?? false;
            PerfEndAlertEnabled = _perf?.EndAlertEnabled ?? false;

            ObjTitle = _obj?.Title ?? string.Empty;
            ObjStartDate = _obj?.StartDate ?? StartDate;
            ObjEndDate = _obj?.DueDate ?? EndDate;
            ObjStartAlertEnabled = _obj?.StartAlertEnabled ?? false;
            ObjEndAlertEnabled = _obj?.EndAlertEnabled ?? false;
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetProperty(ref _isBusy, value))
                {
                    (SaveCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                    (DeleteCommand as AsyncRelayCommand)?.NotifyCanExecuteChanged();
                }
            }
        }

        public IReadOnlyList<string> StatusOptions { get; } = new[] { "Plan to take", "In progress", "Completed", "Dropped" };

        private string _title = string.Empty;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private DateTime _startDate;
        public DateTime StartDate { get => _startDate; set { if (SetProperty(ref _startDate, value)) if (EndDate < _startDate) EndDate = _startDate; } }

        private DateTime _endDate;
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        private string _selectedStatus = "Plan to take";
        public string SelectedStatus { get => _selectedStatus; set => SetProperty(ref _selectedStatus, value); }

        private string _instructorName = string.Empty;
        public string InstructorName { get => _instructorName; set => SetProperty(ref _instructorName, value); }

        private string _instructorPhone = string.Empty;
        public string InstructorPhone { get => _instructorPhone; set => SetProperty(ref _instructorPhone, value); }

        private string _instructorEmail = string.Empty;
        public string InstructorEmail { get => _instructorEmail; set => SetProperty(ref _instructorEmail, value); }

        private string _notes = string.Empty;
        public string Notes { get => _notes; set => SetProperty(ref _notes, value); }

        private bool _startAlertEnabled;
        public bool StartAlertEnabled { get => _startAlertEnabled; set => SetProperty(ref _startAlertEnabled, value); }

        private bool _endAlertEnabled;
        public bool EndAlertEnabled { get => _endAlertEnabled; set => SetProperty(ref _endAlertEnabled, value); }

        private string _perfTitle = string.Empty;
        public string PerfTitle { get => _perfTitle; set => SetProperty(ref _perfTitle, value); }

        private DateTime _perfStartDate = DateTime.Today;
        public DateTime PerfStartDate { get => _perfStartDate; set => SetProperty(ref _perfStartDate, value); }

        private DateTime _perfEndDate = DateTime.Today;
        public DateTime PerfEndDate { get => _perfEndDate; set => SetProperty(ref _perfEndDate, value); }

        private bool _perfStartAlertEnabled;
        public bool PerfStartAlertEnabled { get => _perfStartAlertEnabled; set => SetProperty(ref _perfStartAlertEnabled, value); }

        private bool _perfEndAlertEnabled;
        public bool PerfEndAlertEnabled { get => _perfEndAlertEnabled; set => SetProperty(ref _perfEndAlertEnabled, value); }

      
        private string _objTitle = string.Empty;
        public string ObjTitle { get => _objTitle; set => SetProperty(ref _objTitle, value); }

        private DateTime _objStartDate = DateTime.Today;
        public DateTime ObjStartDate { get => _objStartDate; set => SetProperty(ref _objStartDate, value); }

        private DateTime _objEndDate = DateTime.Today;
        public DateTime ObjEndDate { get => _objEndDate; set => SetProperty(ref _objEndDate, value); }

        private bool _objStartAlertEnabled;
        public bool ObjStartAlertEnabled { get => _objStartAlertEnabled; set => SetProperty(ref _objStartAlertEnabled, value); }

        private bool _objEndAlertEnabled;
        public bool ObjEndAlertEnabled { get => _objEndAlertEnabled; set => SetProperty(ref _objEndAlertEnabled, value); }

        
        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }
        public IRelayCommand ShareNotesCommand { get; }
        public IAsyncRelayCommand DeletePerfCommand { get; }
        public IAsyncRelayCommand DeleteObjCommand { get; }

        private async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (string.IsNullOrWhiteSpace(Title))
                { await Shell.Current.DisplayAlert("Validation", "Please enter a course title.", "OK"); return; }
                if (EndDate < StartDate)
                { await Shell.Current.DisplayAlert("Validation", "End date cannot be before start date.", "OK"); return; }
                if (string.IsNullOrWhiteSpace(InstructorName))
                { await Shell.Current.DisplayAlert("Validation", "Instructor name is required.", "OK"); return; }
                if (!IsValidEmail(InstructorEmail))
                { await Shell.Current.DisplayAlert("Validation", "Please enter a valid email address.", "OK"); return; }
                if (!IsLikelyPhone(InstructorPhone))
                { await Shell.Current.DisplayAlert("Validation", "Please enter a valid phone number.", "OK"); return; }

                _course.Title = Title.Trim();
                _course.StartDate = StartDate;
                _course.EndDate = EndDate;
                _course.Status = FromStatusString(SelectedStatus);
                _course.InstructorName = InstructorName.Trim();
                _course.InstructorPhone = InstructorPhone.Trim();
                _course.InstructorEmail = InstructorEmail.Trim();
                _course.Notes = Notes ?? string.Empty;
                _course.StartAlertEnabled = StartAlertEnabled;
                _course.EndAlertEnabled = EndAlertEnabled;

                await _db.SaveCourseAsync(_course);

                if (_perf == null) _perf = new Assessment { CourseId = _course.Id, Type = AssessmentType.Performance };
                _perf.Title = (PerfTitle ?? string.Empty).Trim();
                _perf.StartDate = PerfStartDate;
                _perf.DueDate = PerfEndDate;
                _perf.StartAlertEnabled = PerfStartAlertEnabled;
                _perf.EndAlertEnabled = PerfEndAlertEnabled;
                await _db.SaveAssessmentAsync(_perf);

                if (_obj == null) _obj = new Assessment { CourseId = _course.Id, Type = AssessmentType.Objective };
                _obj.Title = (ObjTitle ?? string.Empty).Trim();
                _obj.StartDate = ObjStartDate;
                _obj.DueDate = ObjEndDate;
                _obj.StartAlertEnabled = ObjStartAlertEnabled;
                _obj.EndAlertEnabled = ObjEndAlertEnabled;
                await _db.SaveAssessmentAsync(_obj);

                await ScheduleOrCancelAlertsAsync();
                await ScheduleOrCancelAssessmentAlertsAsync();

                await Shell.Current.DisplayAlert("Saved", "Course and assessments saved.", "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error", $"Could not save:\n{ex.Message}", "OK"); }
            finally { IsBusy = false; }
        }

        private async Task DeleteAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (_course.Id == 0)
                { await Shell.Current.Navigation.PopAsync(); return; }

                var ok = await Shell.Current.DisplayAlert("Delete", $"Delete '{_course.Title}' and its assessments?", "Yes", "No");
                if (!ok) return;

                CancelCourseAlerts();
                var list = await _db.GetAssessmentsForCourseAsync(_course.Id);
                foreach (var a in list)
                {
                    LocalNotificationCenter.Current.Cancel(100000 + a.Id);
                    LocalNotificationCenter.Current.Cancel(200000 + a.Id);
                    await _db.DeleteAssessmentAsync(a);
                }
                await _db.DeleteCourseAsync(_course);
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            { await Shell.Current.DisplayAlert("Error", $"Could not delete course:\n{ex.Message}", "OK"); }
            finally { IsBusy = false; }
        }

        private async void ShareNotes()
        {
            if (string.IsNullOrWhiteSpace(Notes))
            { await Shell.Current.DisplayAlert("Share Notes", "There are no notes to share.", "OK"); return; }
            await Share.RequestAsync(new ShareTextRequest { Title = $"Notes for {Title}", Text = Notes });
        }

        private async Task DeletePerfAsync()
        {
            if (_perf != null)
            {
                LocalNotificationCenter.Current.Cancel(100000 + _perf.Id);
                await _db.DeleteAssessmentAsync(_perf);
                _perf = null;
                PerfTitle = string.Empty;
                PerfStartDate = StartDate;
                PerfEndDate = EndDate;
                PerfStartAlertEnabled = false;
                PerfEndAlertEnabled = false;
            }
        }

        private async Task DeleteObjAsync()
        {
            if (_obj != null)
            {
                LocalNotificationCenter.Current.Cancel(200000 + _obj.Id);
                await _db.DeleteAssessmentAsync(_obj);
                _obj = null;
                ObjTitle = string.Empty;
                ObjStartDate = StartDate;
                ObjEndDate = EndDate;
                ObjStartAlertEnabled = false;
                ObjEndAlertEnabled = false;
            }
        }

        private int StartNotificationId => 50000 + (_course.Id * 2);
        private int EndNotificationId => 50001 + (_course.Id * 2);

        private async Task ScheduleOrCancelAlertsAsync()
        {
            var startNotify = new DateTime(StartDate.Year, StartDate.Month, StartDate.Day, 9, 0, 0, DateTimeKind.Local);
            var endNotify = new DateTime(EndDate.Year, EndDate.Month, EndDate.Day, 9, 0, 0, DateTimeKind.Local);

            if (StartAlertEnabled)
                await ScheduleOnceAsync(StartNotificationId, "Course starts", $"{Title} starts today.", startNotify);
            else
                LocalNotificationCenter.Current.Cancel(StartNotificationId);

            if (EndAlertEnabled)
                await ScheduleOnceAsync(EndNotificationId, "Course ends", $"{Title} ends today.", endNotify);
            else
                LocalNotificationCenter.Current.Cancel(EndNotificationId);
        }

        private async Task ScheduleOrCancelAssessmentAlertsAsync()
        {
            if (_perf != null)
            {
                int startId = 1000000 + _perf.Id;
                int endId = 1100000 + _perf.Id;

                if (PerfStartAlertEnabled)
                    await ScheduleOnceAsync(startId, "Assessment starts", $"{_perf.Title} starts today.", PerfStartDate);
                else LocalNotificationCenter.Current.Cancel(startId);

                if (PerfEndAlertEnabled)
                    await ScheduleOnceAsync(endId, "Assessment due", $"{_perf.Title} is due today.", PerfEndDate);
                else LocalNotificationCenter.Current.Cancel(endId);
            }

            if (_obj != null)
            {
                int startId = 1200000 + _obj.Id;
                int endId = 1300000 + _obj.Id;

                if (ObjStartAlertEnabled)
                    await ScheduleOnceAsync(startId, "Assessment starts", $"{_obj.Title} starts today.", ObjStartDate);
                else LocalNotificationCenter.Current.Cancel(startId);

                if (ObjEndAlertEnabled)
                    await ScheduleOnceAsync(endId, "Assessment due", $"{_obj.Title} is due today.", ObjEndDate);
                else LocalNotificationCenter.Current.Cancel(endId);
            }
        }

        private static async Task ScheduleOnceAsync(int id, string title, string description, DateTime whenLocal)
        {
            if (whenLocal <= DateTime.Now)
            { LocalNotificationCenter.Current.Cancel(id); return; }
            var request = new NotificationRequest
            {
                NotificationId = id,
                Title = title,
                Description = description,
                Schedule = new NotificationRequestSchedule { NotifyTime = whenLocal }
            };
            await LocalNotificationCenter.Current.Show(request);
        }

        private void CancelCourseAlerts()
        {
            LocalNotificationCenter.Current.Cancel(StartNotificationId);
            LocalNotificationCenter.Current.Cancel(EndNotificationId);
        }

        private static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.CultureInvariant);
        }

        private static bool IsLikelyPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            var digits = Regex.Replace(phone, @"\D", "");
            return digits.Length >= 10;
        }

        private static string ToStatusString(CourseStatus status) => status switch
        {
            CourseStatus.InProgress => "In progress",
            CourseStatus.Completed => "Completed",
            CourseStatus.Dropped => "Dropped",
            _ => "Plan to take"
        };

        private static CourseStatus FromStatusString(string s) => s switch
        {
            "In progress" => CourseStatus.InProgress,
            "Completed" => CourseStatus.Completed,
            "Dropped" => CourseStatus.Dropped,
            _ => CourseStatus.PlanToTake
        };
    }
}