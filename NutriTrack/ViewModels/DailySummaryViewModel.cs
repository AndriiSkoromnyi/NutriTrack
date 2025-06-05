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
                    OnPropertyChanged(nameof(ProteinProgress));
                    OnPropertyChanged(nameof(RemainingProtein));
                    OnPropertyChanged(nameof(IsProteinGoalMet));
                    OnPropertyChanged(nameof(ProteinStatusMessage));
                    OnPropertyChanged(nameof(FatProgress));
                    OnPropertyChanged(nameof(RemainingFat));
                    OnPropertyChanged(nameof(IsFatGoalMet));
                    OnPropertyChanged(nameof(FatStatusMessage));
                    OnPropertyChanged(nameof(CarbsProgress));
                    OnPropertyChanged(nameof(RemainingCarbs));
                    OnPropertyChanged(nameof(IsCarbsGoalMet));
                    OnPropertyChanged(nameof(CarbsStatusMessage));
                }
            }
        }

        private UserSettings _userSettings;
        public UserSettings UserSettings
        {
            get => _userSettings;
            set
            {
                if (SetProperty(ref _userSettings, value))
                {
                    OnPropertyChanged(nameof(CaloriesProgress));
                    OnPropertyChanged(nameof(RemainingCalories));
                    OnPropertyChanged(nameof(IsCaloriesGoalMet));
                    OnPropertyChanged(nameof(CaloriesStatusMessage));
                    OnPropertyChanged(nameof(ProteinProgress));
                    OnPropertyChanged(nameof(RemainingProtein));
                    OnPropertyChanged(nameof(IsProteinGoalMet));
                    OnPropertyChanged(nameof(ProteinStatusMessage));
                    OnPropertyChanged(nameof(FatProgress));
                    OnPropertyChanged(nameof(RemainingFat));
                    OnPropertyChanged(nameof(IsFatGoalMet));
                    OnPropertyChanged(nameof(FatStatusMessage));
                    OnPropertyChanged(nameof(CarbsProgress));
                    OnPropertyChanged(nameof(RemainingCarbs));
                    OnPropertyChanged(nameof(IsCarbsGoalMet));
                    OnPropertyChanged(nameof(CarbsStatusMessage));
                }
            }
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

        public double ProteinProgress => UserSettings?.DailyProteinGoal > 0 
            ? (Summary?.TotalProtein ?? 0) / UserSettings.DailyProteinGoal * 100 
            : 0;

        public double RemainingProtein => UserSettings?.DailyProteinGoal > 0 
            ? Math.Max(0, UserSettings.DailyProteinGoal - (Summary?.TotalProtein ?? 0))
            : 0;

        public bool IsProteinGoalMet => UserSettings?.DailyProteinGoal > 0 
            && (Summary?.TotalProtein ?? 0) >= UserSettings.DailyProteinGoal;

        public string ProteinStatusMessage
        {
            get
            {
                if (UserSettings?.DailyProteinGoal <= 0)
                    return "Set your daily protein goal in Settings";
                
                if (IsProteinGoalMet)
                    return "💪 Great job! Protein goal achieved!";
                
                return $"Still need {RemainingProtein:F0}g of protein";
            }
        }

        public double FatProgress => UserSettings?.DailyFatGoal > 0 
            ? (Summary?.TotalFat ?? 0) / UserSettings.DailyFatGoal * 100 
            : 0;

        public double RemainingFat => UserSettings?.DailyFatGoal > 0 
            ? Math.Max(0, UserSettings.DailyFatGoal - (Summary?.TotalFat ?? 0))
            : 0;

        public bool IsFatGoalMet => UserSettings?.DailyFatGoal > 0 
            && (Summary?.TotalFat ?? 0) >= UserSettings.DailyFatGoal;

        public string FatStatusMessage
        {
            get
            {
                if (UserSettings?.DailyFatGoal <= 0)
                    return "Set your daily fat goal in Settings";
                
                if (IsFatGoalMet)
                    return "✨ Perfect! Fat goal reached!";
                
                return $"Still need {RemainingFat:F0}g of fat";
            }
        }

        public double CarbsProgress => UserSettings?.DailyCarbsGoal > 0 
            ? (Summary?.TotalCarbohydrates ?? 0) / UserSettings.DailyCarbsGoal * 100 
            : 0;

        public double RemainingCarbs => UserSettings?.DailyCarbsGoal > 0 
            ? Math.Max(0, UserSettings.DailyCarbsGoal - (Summary?.TotalCarbohydrates ?? 0))
            : 0;

        public bool IsCarbsGoalMet => UserSettings?.DailyCarbsGoal > 0 
            && (Summary?.TotalCarbohydrates ?? 0) >= UserSettings.DailyCarbsGoal;

        public string CarbsStatusMessage
        {
            get
            {
                if (UserSettings?.DailyCarbsGoal <= 0)
                    return "Set your daily carbs goal in Settings";
                
                if (IsCarbsGoalMet)
                    return "🌟 Excellent! Carbs goal achieved!";
                
                return $"Still need {RemainingCarbs:F0}g of carbs";
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
            OnPropertyChanged(nameof(ProteinProgress));
            OnPropertyChanged(nameof(RemainingProtein));
            OnPropertyChanged(nameof(IsProteinGoalMet));
            OnPropertyChanged(nameof(ProteinStatusMessage));
            OnPropertyChanged(nameof(FatProgress));
            OnPropertyChanged(nameof(RemainingFat));
            OnPropertyChanged(nameof(IsFatGoalMet));
            OnPropertyChanged(nameof(FatStatusMessage));
            OnPropertyChanged(nameof(CarbsProgress));
            OnPropertyChanged(nameof(RemainingCarbs));
            OnPropertyChanged(nameof(IsCarbsGoalMet));
            OnPropertyChanged(nameof(CarbsStatusMessage));
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