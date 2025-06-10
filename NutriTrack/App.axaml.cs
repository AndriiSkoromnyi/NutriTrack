using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using NutriTrack.Views;
using NutriTrack.Services;
using NutriTrack.ViewModels;

namespace NutriTrack
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // Entry point after application initialization
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Create services
                var productService = new ProductService();
                var mealEntryService = new MealEntryService();
                var dailySummaryService = new DailySummaryService();
                var userSettingsService = new UserSettingsService();
                var weightConversionService = new WeightConversionService();

                // Create main ViewModel with service injection
                var mainViewModel = new MainViewModel(
                    productService, 
                    mealEntryService, 
                    dailySummaryService, 
                    userSettingsService,
                    weightConversionService);

                // Create main window and set DataContext
                desktop.MainWindow = new MainView
                {
                    DataContext = mainViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}