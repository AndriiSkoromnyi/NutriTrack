using Avalonia.Controls;
using NutriTrack.ViewModels;

namespace NutriTrack.Views
{
    public partial class MealEntryView : UserControl
    {
        public MealEntryView()
        {
            InitializeComponent();
            // DataContext обычно устанавливается из MainViewModel через ContentControl,
            // но если нужно, можно раскомментировать и задать напрямую:
            // DataContext = new MealEntryViewModel(new Services.MealEntryService(), new Services.ProductService());
        }
    }
}