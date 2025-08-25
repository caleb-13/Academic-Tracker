using Microsoft.Extensions.Logging;
using Mobile_Application_Development.Data;
using Mobile_Application_Development.ViewModels;
using Mobile_Application_Development.Views;
using Plugin.LocalNotification; 

namespace Mobile_Application_Development
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            builder
                .UseMauiApp<App>()
                .UseLocalNotification() 
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            
            builder.Services.AddSingleton<TermDatabase>();

            
            builder.Services.AddTransient<TermsViewModel>();
            builder.Services.AddTransient<CourseDetailViewModel>();

            
            builder.Services.AddTransient<TermsPage>();
            builder.Services.AddTransient<TermDetailPage>();
            builder.Services.AddTransient<CourseDetailPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            
            _ = app.Services.GetRequiredService<TermDatabase>();

            return app;
        }
    }
}
