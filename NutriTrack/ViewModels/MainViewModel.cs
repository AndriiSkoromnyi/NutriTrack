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

            ShowProductsCommand = new RelayCommand(() => CurrentViewModel = new ProductViewModel(_productService));
            ShowMealEntriesCommand = new RelayCommand(() => CurrentViewModel = new MealEntryViewModel(_mealEntryService, _productService));
            ShowDailySummaryCommand = new RelayCommand(() => CurrentViewModel = new DailySummaryViewModel(_dailySummaryService, _mealEntryService, _productService));
            ShowSettingsCommand = new RelayCommand(() => CurrentViewModel = new SettingsViewModel(_userSettingsService));

            // По умолчанию показываем продукты
            CurrentViewModel = new ProductViewModel(_productService);
        }
    }
}
