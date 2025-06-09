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
        private readonly IWeightConversionService _weightConversionService;

        // Cached ViewModels
        private readonly ProductViewModel _productViewModel;
        private readonly MealEntryViewModel _mealEntryViewModel;
        private readonly DailySummaryViewModel _dailySummaryViewModel;
        private readonly SettingsViewModel _settingsViewModel;

        private ViewModelBase _currentViewModel;
        public ViewModelBase CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public ICommand ShowProductsCommand { get; }
        public ICommand ShowMealEntriesCommand { get; }
        public ICommand ShowDailySummaryCommand { get; }
        public ICommand ShowSettingsCommand { get; }

        public MainViewModel(
            IProductService productService,
            IMealEntryService mealEntryService,
            IDailySummaryService dailySummaryService,
            IUserSettingsService userSettingsService,
            IWeightConversionService weightConversionService)
        {
            _productService = productService;
            _mealEntryService = mealEntryService;
            _dailySummaryService = dailySummaryService;
            _userSettingsService = userSettingsService;
            _weightConversionService = weightConversionService;

            // Initialize cached ViewModels
            _productViewModel = new ProductViewModel(_productService, _userSettingsService, _weightConversionService);
            _mealEntryViewModel = new MealEntryViewModel(_mealEntryService, _productService, _userSettingsService, _weightConversionService);
            _dailySummaryViewModel = new DailySummaryViewModel(_dailySummaryService, _mealEntryService, _productService, _userSettingsService, _weightConversionService);
            _settingsViewModel = new SettingsViewModel(_userSettingsService, _weightConversionService);

            ShowProductsCommand = new RelayCommand(() => CurrentViewModel = _productViewModel);
            ShowMealEntriesCommand = new RelayCommand(() => CurrentViewModel = _mealEntryViewModel);
            ShowDailySummaryCommand = new RelayCommand(() => CurrentViewModel = _dailySummaryViewModel);
            ShowSettingsCommand = new RelayCommand(() => CurrentViewModel = _settingsViewModel);

            // Show products by default
            CurrentViewModel = _productViewModel;
        }

        public void ShowMealEntry()
        {
            CurrentViewModel = new MealEntryViewModel(
                _mealEntryService,
                _productService,
                _userSettingsService,
                _weightConversionService);
        }

        public void ShowProducts()
        {
            CurrentViewModel = new ProductViewModel(_productService, _userSettingsService, _weightConversionService);
        }

        public void ShowDailySummary()
        {
            CurrentViewModel = new DailySummaryViewModel(_dailySummaryService, _mealEntryService, _productService, _userSettingsService, _weightConversionService);
        }

        public void ShowSettings()
        {
            CurrentViewModel = new SettingsViewModel(_userSettingsService, _weightConversionService);
        }
    }
}
