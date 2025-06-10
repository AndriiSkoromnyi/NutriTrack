using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Helpers;

namespace NutriTrack.Services
{
    public interface IDailySummaryService
    {
        Task<DailySummary> LoadDailySummaryAsync(DateTime date);
        Task SaveDailySummaryAsync(DailySummary summary);
        Task<DailySummary> CalculateDailySummaryAsync(DateTime date, List<MealEntry> mealEntries, List<Product> products);
    }

    public class DailySummaryService : IDailySummaryService
    {
        private bool _isInitialized;
        private readonly object _lockObject = new object();
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<DailySummary> _dailySummaries;

        public DailySummaryService()
        {
            var dataPath = DataPathHelper.GetDataPath();
            _filePath = Path.Combine(dataPath, "dailySummaries.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new DateTimeConverter() }
            };
            
            _dailySummaries = new List<DailySummary>();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;
                
                try
                {
                    if (File.Exists(_filePath))
                    {
                        var json = File.ReadAllText(_filePath);
                        _dailySummaries = JsonSerializer.Deserialize<List<DailySummary>>(json, _jsonOptions) ?? new List<DailySummary>();
                    }
                    else
                    {
                        _dailySummaries = new List<DailySummary>();
                        var json = JsonSerializer.Serialize(_dailySummaries, _jsonOptions);
                        File.WriteAllText(_filePath, json);
                    }
                }
                catch (Exception ex)
                {
                    _dailySummaries = new List<DailySummary>();
                    Console.WriteLine($"Error initializing daily summaries: {ex.Message}");
                }

                _isInitialized = true;
            }
        }

        public async Task<List<DailySummary>> LoadAllSummariesAsync()
        {
            if (!File.Exists(_filePath))
            {
                _dailySummaries = new List<DailySummary>();
                await SaveAllSummariesAsync(_dailySummaries);
                return _dailySummaries;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            _dailySummaries = JsonSerializer.Deserialize<List<DailySummary>>(json, _jsonOptions) ?? new List<DailySummary>();
            return _dailySummaries;
        }

        public async Task SaveAllSummariesAsync(List<DailySummary> summaries)
        {
            var json = JsonSerializer.Serialize(summaries, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            _dailySummaries = summaries;
        }

        public async Task<DailySummary> LoadDailySummaryAsync(DateTime date)
        {
            await InitializeAsync();
            return _dailySummaries.FirstOrDefault(s => s.Date.Date == date.Date) ?? new DailySummary { Date = date };
        }

        public async Task SaveDailySummaryAsync(DailySummary summary)
        {
            var allSummaries = await LoadAllSummariesAsync();

            var index = allSummaries.FindIndex(s => s.Date.Date == summary.Date.Date);
            if (index >= 0)
            {
                allSummaries[index] = summary;
            }
            else
            {
                allSummaries.Add(summary);
            }

            await SaveAllSummariesAsync(allSummaries);
        }

        public async Task<DailySummary> CalculateDailySummaryAsync(DateTime date, List<MealEntry> mealEntries, List<Product> products)
        {
            var entriesForDate = mealEntries.Where(e => e.Date.Date == date.Date).ToList();

            double totalCalories = 0;
            double totalProtein = 0;
            double totalFat = 0;
            double totalCarbohydrates = 0;

            foreach (var entry in entriesForDate)
            {
                var product = products.FirstOrDefault(p => p.Id == entry.ProductId);
                if (product != null)
                {
                    double factor = entry.Weight / 100.0;
                    totalCalories += product.CaloriesPer100g * factor;
                    totalProtein += product.Protein * factor;
                    totalFat += product.Fat * factor;
                    totalCarbohydrates += product.Carbohydrates * factor;
                }
            }

            var summary = new DailySummary
            {
                Date = date,
                TotalCalories = totalCalories,
                TotalProtein = totalProtein,
                TotalFat = totalFat,
                TotalCarbohydrates = totalCarbohydrates,
                MealEntries = entriesForDate
            };

            await SaveDailySummaryAsync(summary);

            return summary;
        }
    }
}
