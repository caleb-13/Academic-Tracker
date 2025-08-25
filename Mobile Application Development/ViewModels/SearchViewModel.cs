using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Maui.Controls;                    // for Command
using Mobile_Application_Development.Services;

namespace Mobile_Application_Development.ViewModels
{
    public class SearchViewModel : BaseViewModel
    {
        private readonly SearchService _service;

        private string _query;
        public string Query
        {
            get => _query;
            set
            {
                if (_query != value)
                {
                    _query = value;
                    OnPropertyChanged();
                }
            }
        }

        public ObservableCollection<SearchResult> Results { get; } = new();

        public ICommand RunSearchCommand { get; }

        public SearchViewModel()
        {
            // keep it simple: manual new, no DI
            _service = new SearchService(new Data.TermDatabase());
            RunSearchCommand = new Command(async () => await RunSearchAsync());
        }

        private async Task RunSearchAsync()
        {
            Results.Clear();
            if (string.IsNullOrWhiteSpace(Query)) return;

            var found = await _service.SearchAsync(Query);
            foreach (var item in found)
                Results.Add(item);
        }
    }
}
