using Avalonia.Controls;
using NutriTrack.ViewModels;

namespace NutriTrack.Views
{
    public partial class SettingsView : UserControl
    {
        public SettingsView()
        {
            InitializeComponent();
            // DataContext обычно устанавливается из MainViewModel через ContentControl,
            // но если нужно, можно раскомментировать и задать напрямую:
            // DataContext = new SettingsViewModel(new Services.UserSettingsService());
        }
    }
}