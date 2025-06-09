using Avalonia.Controls;
using NutriTrack.ViewModels;

namespace NutriTrack.Views
{
    public partial class MainView : Window
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel(
                new Services.ProductService(),
                new Services.MealEntryService(),
                new Services.DailySummaryService(),
                new Services.UserSettingsService(),
                new Services.WeightConversionService());
        }
    }
}