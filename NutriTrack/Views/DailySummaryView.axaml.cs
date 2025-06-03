using Avalonia.Controls;
using NutriTrack.ViewModels;

namespace NutriTrack.Views
{
    public partial class DailySummaryView : UserControl
    {
        public DailySummaryView()
        {
            InitializeComponent();
            // DataContext обычно устанавливается из MainViewModel через ContentControl,
            // но если нужно, можно раскомментировать и задать напрямую:
            // DataContext = new DailySummaryViewModel(new Services.DailySummaryService(), new Services.MealEntryService(), new Services.ProductService());
        }
    }
}