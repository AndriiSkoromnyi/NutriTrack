using System;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace NutriTrack.ViewModels
{
    public class DailySummaryViewModel : ViewModelBase
    {
        private readonly IDailySummaryService _dailySummaryService;
        private readonly IMealEntryService _mealEntryService;
        private readonly IProductService _productService;
        private readonly IUserSettingsService _userSettingsService;

        private DailySummary _summary;
        public DailySummary Summary
        {
            get => _summary;
            set
            {
                if (SetProperty(ref _summary, value))
                {
                    OnPropertyChanged(nameof(CaloriesProgress));
                    OnPropertyChanged(nameof(RemainingCalories));
                    OnPropertyChanged(nameof(IsCaloriesGoalMet));
                    OnPropertyChanged(nameof(CaloriesStatusMessage));
                }
            }
        }

        private UserSettings _userSettings;
        public UserSettings UserSettings
        {
            get => _userSettings;
            set => SetProperty(ref _userSettings, value);
        }

        public double CaloriesProgress => UserSettings?.DailyCalorieGoal > 0 
            ? (Summary?.TotalCalories ?? 0) / UserSettings.DailyCalorieGoal * 100 
            : 0;

        public double RemainingCalories => UserSettings?.DailyCalorieGoal > 0 
            ? Math.Max(0, UserSettings.DailyCalorieGoal - (Summary?.TotalCalories ?? 0))
            : 0;

        public bool IsCaloriesGoalMet => UserSettings?.DailyCalorieGoal > 0 
            && (Summary?.TotalCalories ?? 0) >= UserSettings.DailyCalorieGoal;

        public string CaloriesStatusMessage
        {
            get
            {
                if (UserSettings?.DailyCalorieGoal <= 0)
                    return "Set your daily calorie goal in Settings";
                
                if (IsCaloriesGoalMet)
                    return "🎉 Congratulations! You've reached your daily calorie goal!";
                
                return $"Still need {RemainingCalories:F0} kcal to reach your goal";
            }
        }

        private DateTimeOffset _selectedDate = DateTimeOffset.Now;
        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                    _ = LoadSummaryAsync();
            }
        }

        public ObservableCollection<MealEntryDisplayModel> MealEntries { get; } = new();

        public IAsyncRelayCommand LoadSummaryCommand { get; }

        public DailySummaryViewModel(IDailySummaryService dailySummaryService,
            IMealEntryService mealEntryService,
            IProductService productService,
            IUserSettingsService userSettingsService)
        {
            _dailySummaryService = dailySummaryService;
            _mealEntryService = mealEntryService;
            _productService = productService;
            _userSettingsService = userSettingsService;

            LoadSummaryCommand = new AsyncRelayCommand(LoadSummaryAsync);

            // Подписываемся на события изменения данных
            _mealEntryService.MealEntriesChanged += async (s, e) => await LoadSummaryAsync();
            _productService.ProductsChanged += async (s, e) => await LoadSummaryAsync();
            _userSettingsService.SettingsChanged += async (s, e) => await LoadUserSettingsAsync();

            _ = LoadUserSettingsAsync();
            _ = LoadSummaryAsync();
        }

        private async Task LoadUserSettingsAsync()
        {
            UserSettings = await _userSettingsService.LoadSettingsAsync();
            // Обновляем сообщения после загрузки настроек
            OnPropertyChanged(nameof(CaloriesProgress));
            OnPropertyChanged(nameof(RemainingCalories));
            OnPropertyChanged(nameof(IsCaloriesGoalMet));
            OnPropertyChanged(nameof(CaloriesStatusMessage));
        }

        private async Task LoadSummaryAsync()
        {
            var mealEntries = await _mealEntryService.LoadMealEntriesAsync(SelectedDate.DateTime);
            var products = await _productService.LoadProductsAsync();

            // Фильтруем записи, оставляя только те, у которых есть существующие продукты
            var validMealEntries = mealEntries
                .Where(entry => products.Any(p => p.Id == entry.ProductId))
                .ToList();

            Summary = await _dailySummaryService.CalculateDailySummaryAsync(SelectedDate.DateTime, validMealEntries, products);
            
            // Обновляем отображаемые записи
            MealEntries.Clear();
            foreach (var entry in validMealEntries.OrderBy(e => e.Date))
            {
                var product = products.First(p => p.Id == entry.ProductId);
                MealEntries.Add(new MealEntryDisplayModel(entry, product));
            }
        }
    }
}