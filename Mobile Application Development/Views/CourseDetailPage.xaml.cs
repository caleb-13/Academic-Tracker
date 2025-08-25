using Mobile_Application_Development.Data;
using Mobile_Application_Development.Models;
using Mobile_Application_Development.ViewModels;

namespace Mobile_Application_Development.Views
{
    public partial class CourseDetailPage : ContentPage
    {
        public CourseDetailPage(TermDatabase db, Course course)
        {
            InitializeComponent();
            BindingContext = new CourseDetailViewModel(db, course);
        }
    }
}
