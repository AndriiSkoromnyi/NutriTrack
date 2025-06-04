using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using NutriTrack.Models;

namespace NutriTrack.Services
{
    public interface IMealEntryService
    {
        Task<List<MealEntry>> LoadMealEntriesAsync(DateTime date);
        Task SaveMealEntriesAsync(List<MealEntry> mealEntries);
        Task AddMealEntryAsync(MealEntry entry);
        Task UpdateMealEntryAsync(MealEntry entry);
        Task DeleteMealEntryAsync(Guid entryId);
        event EventHandler MealEntriesChanged;
    }

    public class MealEntryService : IMealEntryService
    {
        private readonly string _filePath;
        private List<MealEntry> _mealEntries = new List<MealEntry>();
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler MealEntriesChanged;

        public MealEntryService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NutriTrack"
            );
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "mealEntries.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new DateTimeJsonConverter() }
            };
        }

        public async Task<List<MealEntry>> LoadMealEntriesAsync(DateTime date)
        {
            if (!File.Exists(_filePath))
            {
                _mealEntries = new List<MealEntry>();
                await SaveMealEntriesAsync(_mealEntries);
                return _mealEntries.Where(e => e.Date.Date == date.Date).ToList();
            }

            var json = await File.ReadAllTextAsync(_filePath);
            _mealEntries = JsonSerializer.Deserialize<List<MealEntry>>(json, _jsonOptions) ?? new List<MealEntry>();

            return _mealEntries.Where(e => e.Date.Date == date.Date).ToList();
        }

        public async Task SaveMealEntriesAsync(List<MealEntry> mealEntries)
        {
            var json = JsonSerializer.Serialize(mealEntries, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            _mealEntries = mealEntries;
            MealEntriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddMealEntryAsync(MealEntry entry)
        {
            await LoadAllMealEntriesAsync();
            _mealEntries.Add(entry);
            await SaveMealEntriesAsync(_mealEntries);
            MealEntriesChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task UpdateMealEntryAsync(MealEntry entry)
        {
            await LoadAllMealEntriesAsync();
            var index = _mealEntries.FindIndex(e => e.Id == entry.Id);
            if (index >= 0)
            {
                _mealEntries[index] = entry;
                await SaveMealEntriesAsync(_mealEntries);
                MealEntriesChanged?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                throw new ArgumentException("Meal entry not found");
            }
        }

        public async Task DeleteMealEntryAsync(Guid entryId)
        {
            await LoadAllMealEntriesAsync();
            _mealEntries.RemoveAll(e => e.Id == entryId);
            await SaveMealEntriesAsync(_mealEntries);
            MealEntriesChanged?.Invoke(this, EventArgs.Empty);
        }

        private async Task LoadAllMealEntriesAsync()
        {
            if (!File.Exists(_filePath))
            {
                _mealEntries = new List<MealEntry>();
                await SaveMealEntriesAsync(_mealEntries);
                return;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            _mealEntries = JsonSerializer.Deserialize<List<MealEntry>>(json, _jsonOptions) ?? new List<MealEntry>();
        }
    }

    public class DateTimeJsonConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dateString = reader.GetString();
            return DateTime.Parse(dateString ?? DateTime.Now.ToString("O"));
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}
