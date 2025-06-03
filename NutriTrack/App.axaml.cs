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

        // Точка входа после инициализации приложения
        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // Создаем сервисы
                var productService = new ProductService();
                var mealEntryService = new MealEntryService();
                var dailySummaryService = new DailySummaryService();
                var userSettingsService = new UserSettingsService();

                // Создаем главный ViewModel с внедрением сервисов
                var mainViewModel = new MainViewModel(productService, mealEntryService, dailySummaryService, userSettingsService);

                // Создаем главное окно и задаем DataContext
                desktop.MainWindow = new MainView
                {
                    DataContext = mainViewModel
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}