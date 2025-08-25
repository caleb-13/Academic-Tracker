using System.Linq;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;
using Mobile_Application_Development.ViewModels;

namespace Mobile_Application_Development.Views
{
    public partial class TermsPage : ContentPage
    {
        private readonly TermDatabase _db;
        private readonly TermsViewModel _vm;

        public TermsPage(TermDatabase db, TermsViewModel vm)
        {
            InitializeComponent();
            _db = db;
            _vm = vm;
            BindingContext = _vm;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _vm.LoadAsync();
        }

        private async void OnTermSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection?.FirstOrDefault() is Term selected)
            {
                await Navigation.PushAsync(new TermDetailPage(_db, selected));
                ((CollectionView)sender).SelectedItem = null;
            }
        }

        
        private async void OnTermEditClicked(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var term = button.CommandParameter as Term;
            if (term != null)
            {
                await Navigation.PushAsync(new TermDetailPage(_db, term));
            }
        }
    }
}