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

        private UserSettings _settings;
        public UserSettings Settings
        {
            get => _settings;
            set => SetProperty(ref _settings, value);
        }

        public ObservableCollection<WeightUnit> WeightUnits { get; }

        public IAsyncRelayCommand LoadSettingsCommand { get; }
        public IAsyncRelayCommand SaveSettingsCommand { get; }

        public SettingsViewModel(IUserSettingsService userSettingsService)
        {
            _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
            
            WeightUnits = new ObservableCollection<WeightUnit>(
                Enum.GetValues(typeof(WeightUnit)).Cast<WeightUnit>());

            LoadSettingsCommand = new AsyncRelayCommand(LoadSettingsAsync);
            SaveSettingsCommand = new AsyncRelayCommand(SaveSettingsAsync);
            
            _ = LoadSettingsAsync();
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
