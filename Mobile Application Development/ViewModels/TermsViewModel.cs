using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mobile_Application_Development.Models;
using Mobile_Application_Development.Data;

namespace Mobile_Application_Development.ViewModels
{
   
    public partial class TermsViewModel : ObservableObject
    {
        private readonly TermDatabase _db;

        public ObservableCollection<Term> Terms { get; } = new();

        
        private string _title = "";
        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        private DateTime _startDate = DateTime.Today;
        public DateTime StartDate
        {
            get => _startDate;
            set => SetProperty(ref _startDate, value);
        }

        private DateTime _endDate = DateTime.Today.AddDays(1);
        public DateTime EndDate
        {
            get => _endDate;
            set => SetProperty(ref _endDate, value);
        }

        private Term? _selected;
        public Term? Selected
        {
            get => _selected;
            set => SetProperty(ref _selected, value);
        }

        
        public IAsyncRelayCommand SaveCommand { get; }
        public IRelayCommand<Term> EditCommand { get; }
        public IAsyncRelayCommand<Term> DeleteCommand { get; }

        public TermsViewModel(TermDatabase db)
        {
            _db = db;

            SaveCommand = new AsyncRelayCommand(SaveAsync);
            EditCommand = new RelayCommand<Term>(Edit);
            DeleteCommand = new AsyncRelayCommand<Term>(DeleteAsync);
        }

        public async Task LoadAsync()
        {
            Terms.Clear();
            foreach (var t in await _db.GetTermsAsync())
                Terms.Add(t);
        }

        private async Task SaveAsync()
        {
            

            if (string.IsNullOrWhiteSpace(Title))
            {
                await Shell.Current.DisplayAlert("Validation", "Title is required.", "OK");
                return;
            }
            if (EndDate < StartDate)
            {
                await Shell.Current.DisplayAlert("Validation", "End date cannot be before start date.", "OK");
                return;
            }

            var term = Selected ?? new Term();
            term.Title = Title.Trim();
            term.StartDate = StartDate;
            term.EndDate = EndDate;

            await _db.SaveAsync(term);
            await LoadAsync();
            ClearForm();
        }

        private void Edit(Term term)
        {
            Selected = term;
            Title = term.Title;
            StartDate = term.StartDate;
            EndDate = term.EndDate;
        }

        private async Task DeleteAsync(Term term)
        {
            var confirm = await Shell.Current.DisplayAlert("Delete", $"Delete '{term.Title}'?", "Yes", "No");
            if (!confirm) return;

            await _db.DeleteTermAsync(term);
            await LoadAsync();

            if (Selected?.Id == term.Id)
                ClearForm();
        }

        private void ClearForm()
        {
            Selected = null;
            Title = "";
            StartDate = DateTime.Today;
            EndDate = DateTime.Today.AddDays(1);
        }
    }
}
