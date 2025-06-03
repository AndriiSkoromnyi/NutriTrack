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
    }

    public class UserSettingsService : IUserSettingsService
    {
        private readonly string _filePath = "usersettings.json";

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
            var settings = JsonSerializer.Deserialize<UserSettings>(json);
            return settings ?? new UserSettings { DailyCalorieGoal = 2000, WeightUnit = WeightUnit.Grams };
        }

        public async Task SaveSettingsAsync(UserSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
        }
    }
}