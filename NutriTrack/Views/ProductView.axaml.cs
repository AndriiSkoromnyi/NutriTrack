using Avalonia.Controls;
using NutriTrack.ViewModels;

namespace NutriTrack.Views
{
    public partial class ProductView : UserControl
    {
        public ProductView()
        {
            InitializeComponent();
            // DataContext обычно устанавливается из MainViewModel через ContentControl,
            // но если нужно, можно раскомментировать и задать напрямую:
            // DataContext = new ProductViewModel(new Services.ProductService());
        }
    }
}