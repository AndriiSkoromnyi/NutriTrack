using System.Windows.Input;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;

namespace NutriTrack.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly IMealEntryService _mealEntryService;
        private readonly IDailySummaryService _dailySummaryService;
        private readonly IUserSettingsService _userSettingsService;

        // Cached ViewModels
        private readonly ProductViewModel _productViewModel;
        private readonly MealEntryViewModel _mealEntryViewModel;
        private readonly DailySummaryViewModel _dailySummaryViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ShowProductsCommand { get; }
        public ICommand ShowMealEntriesCommand { get; }
        public ICommand ShowDailySummaryCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(IProductService productService,
                             IMealEntryService mealEntryService,
                             IDailySummaryService dailySummaryService,
                             IUserSettingsService userSettingsService)
        {
            _productService = productService;
            _mealEntryService = mealEntryService;
            _dailySummaryService = dailySummaryService;
            _userSettingsService = userSettingsService;

            // Initialize cached ViewModels
            _productViewModel = new ProductViewModel(_productService);
            _mealEntryViewModel = new MealEntryViewModel(_mealEntryService, _productService);
            _dailySummaryViewModel = new DailySummaryViewModel(_dailySummaryService, _mealEntryService, _productService);
            _settingsViewModel = new SettingsViewModel(_userSettingsService);

            ShowProductsCommand = new RelayCommand(() => CurrentViewModel = _productViewModel);
            ShowMealEntriesCommand = new RelayCommand(() => CurrentViewModel = _mealEntryViewModel);
            ShowDailySummaryCommand = new RelayCommand(() => CurrentViewModel = _dailySummaryViewModel);
            ShowSettingsCommand = new RelayCommand(() => CurrentViewModel = _settingsViewModel);

            // Show products by default
            CurrentViewModel = _productViewModel;
        }
    }
}
