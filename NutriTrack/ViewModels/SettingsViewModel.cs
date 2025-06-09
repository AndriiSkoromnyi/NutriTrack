using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace NutriTrack.ViewModels
{
    public class SettingsViewModel : ViewModelBase
    {
        private readonly IUserSettingsService _userSettingsService;
        private readonly IWeightConversionService _weightConversionService;

        private UserSettings _settings;
        public UserSettings Settings
        {
            get => _settings;
            set
            {
                var oldSettings = _settings;
                if (SetProperty(ref _settings, value) && oldSettings != null && value != null)
                {
                    // If weight unit changed, convert all goals
                    if (oldSettings.WeightUnit != value.WeightUnit)
                    {
                        ConvertGoalsToNewUnit(oldSettings.WeightUnit, value.WeightUnit);
                        OnPropertyChanged(nameof(WeightUnitDisplay));
                    }
                }
            }
        }

        public string WeightUnitDisplay => Settings?.WeightUnit == WeightUnit.Grams ? "g" : "oz";

        public ObservableCollection<WeightUnit> WeightUnits { get; }

        public IAsyncRelayCommand LoadSettingsCommand { get; }
        public IAsyncRelayCommand SaveSettingsCommand { get; }

        public SettingsViewModel(IUserSettingsService userSettingsService, IWeightConversionService weightConversionService)
        {
            _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
            _weightConversionService = weightConversionService ?? throw new ArgumentNullException(nameof(weightConversionService));
            
            WeightUnits = new ObservableCollection<WeightUnit>(
                Enum.GetValues(typeof(WeightUnit)).Cast<WeightUnit>());

            LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            
            _ = LoadSettingsAsync();
        }

        private void ConvertGoalsToNewUnit(WeightUnit fromUnit, WeightUnit toUnit)
        {
            if (Settings == null) return;

            Settings.DailyProteinGoal = _weightConversionService.Convert(Settings.DailyProteinGoal, fromUnit, toUnit);
            Settings.DailyFatGoal = _weightConversionService.Convert(Settings.DailyFatGoal, fromUnit, toUnit);
            Settings.DailyCarbsGoal = _weightConversionService.Convert(Settings.DailyCarbsGoal, fromUnit, toUnit);
            
            // Notify UI of changes
            OnPropertyChanged(nameof(Settings));
        }

        private async Task LoadSettingsAsync()
        {
            var loadedSettings = await _userSettingsService.LoadSettingsAsync();
            if (loadedSettings != null)
            {
                Settings = loadedSettings;
            }
            else
            {
                Settings = new UserSettings
                {
                    DailyCalorieGoal = 2000,
                    WeightUnit = WeightUnit.Grams
                };
            }
        }

        private async Task SaveSettingsAsync()
        {
            if (Settings != null)
            {
                await _userSettingsService.SaveSettingsAsync(Settings);
            }
        }
    }
}
