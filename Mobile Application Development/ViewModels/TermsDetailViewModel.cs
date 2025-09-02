using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;

namespace Mobile_Application_Development.ViewModels
{
    public class TermDetailViewModel : ObservableObject
    {
        private readonly TermDatabase _db;
        private readonly Term _term;

        public TermDetailViewModel(TermDatabase db, Term? term = null)
        {
            _db = db;
            _term = term ?? new Term
            {
                Title = string.Empty,
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1)
            };

            Title = _term.Title ?? string.Empty;
            StartDate = _term.StartDate == default ? DateTime.Today : _term.StartDate;
            EndDate = _term.EndDate == default ? DateTime.Today.AddDays(1) : _term.EndDate;

            SaveCommand = new AsyncRelayCommand(SaveAsync, () => !IsBusy);
            DeleteCommand = new AsyncRelayCommand(DeleteAsync, () => !IsBusy);
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

        private string _title = string.Empty;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private DateTime _startDate;
        public DateTime StartDate
        {
            get => _startDate;
            set
            {
                if (SetProperty(ref _startDate, value))
                {
                    if (EndDate < _startDate) EndDate = _startDate;
                }
            }
        }

        private DateTime _endDate;
        public DateTime EndDate { get => _endDate; set => SetProperty(ref _endDate, value); }

        public IAsyncRelayCommand SaveCommand { get; }
        public IAsyncRelayCommand DeleteCommand { get; }

        
        private static string Clean(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            var trimmed = s.Trim();
            return Regex.Replace(trimmed, @"\s{2,}", " ");
        }

     
        private async Task SaveAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                var errors = new List<string>();

                var titleClean = Clean(Title);

                if (string.IsNullOrWhiteSpace(titleClean))
                    errors.Add("Please enter a term title.");

                if (EndDate < StartDate)
                    errors.Add("Term end date cannot be before start date.");

              
                if (_term.Id != 0)
                {
                    var courses = await _db.GetCoursesForTermAsync(_term.Id);
                    foreach (var c in courses)
                    {
                        if (c.StartDate < StartDate || c.EndDate > EndDate)
                        {
                            errors.Add($"Course '{c.Title}' falls outside the new term dates.");
                            break;
                        }
                    }
                }

                if (errors.Count > 0)
                {
                    await Shell.Current.DisplayAlert("Please fix the following", "• " + string.Join("\n• ", errors), "OK");
                    return;
                }

                _term.Title = titleClean;
                _term.StartDate = StartDate;
                _term.EndDate = EndDate;

                await _db.SaveTermAsync(_term);
                await Shell.Current.DisplayAlert("Saved", "Term saved successfully.", "OK");
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not save term:\n{ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }

        private async Task DeleteAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            try
            {
                if (_term.Id == 0) { await Shell.Current.Navigation.PopAsync(); return; }

                var ok = await Shell.Current.DisplayAlert("Delete",
                    $"Delete '{_term.Title}' and all its courses/assessments?", "Yes", "No");
                if (!ok) return;

                await _db.DeleteTermAsync(_term);
                await Shell.Current.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"Could not delete term:\n{ex.Message}", "OK");
            }
            finally { IsBusy = false; }
        }
    }
}
