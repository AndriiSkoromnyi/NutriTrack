using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using NutriTrack.Models;

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
        private readonly string _filePath;
        private List<DailySummary> _dailySummaries = new List<DailySummary>();

        public DailySummaryService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NutriTrack"
            );
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "dailySummaries.json");
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
            _dailySummaries = JsonSerializer.Deserialize<List<DailySummary>>(json) ?? new List<DailySummary>();
            return _dailySummaries;
        }

        public async Task SaveAllSummariesAsync(List<DailySummary> summaries)
        {
            var json = JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
            _dailySummaries = summaries;
        }

        public async Task<DailySummary> LoadDailySummaryAsync(DateTime date)
        {
            var allSummaries = await LoadAllSummariesAsync();
            return allSummaries.FirstOrDefault(s => s.Date.Date == date.Date) ?? new DailySummary { Date = date };
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

            // Optionally save summary
            await SaveDailySummaryAsync(summary);

            return summary;
        }
    }
}
