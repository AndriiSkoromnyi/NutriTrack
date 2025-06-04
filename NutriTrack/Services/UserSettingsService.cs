using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using NutriTrack.Models;

namespace NutriTrack.Services
{
    public interface IUserSettingsService
    {
        Task<UserSettings> LoadSettingsAsync();
        Task SaveSettingsAsync(UserSettings settings);
        event EventHandler SettingsChanged;
    }

    public class UserSettingsService : IUserSettingsService
    {
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler SettingsChanged;

        public UserSettingsService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NutriTrack"
            );
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "settings.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        public async Task<UserSettings> LoadSettingsAsync()
        {
            if (!File.Exists(_filePath))
            {
                var defaultSettings = new UserSettings
                {
                    DailyCalorieGoal = 2000,
                    WeightUnit = WeightUnit.Grams
                };
                await SaveSettingsAsync(defaultSettings);
                return defaultSettings;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            return JsonSerializer.Deserialize<UserSettings>(json, _jsonOptions) ?? new UserSettings();
        }

        public async Task SaveSettingsAsync(UserSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            SettingsChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}