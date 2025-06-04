using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
    }

    public class MealEntryService : IMealEntryService
    {
        private readonly string _filePath;
        private List<MealEntry> _mealEntries = new List<MealEntry>();

        public MealEntryService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "NutriTrack"
            );
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "mealEntries.json");
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
            _mealEntries = JsonSerializer.Deserialize<List<MealEntry>>(json) ?? new List<MealEntry>();

            return _mealEntries.Where(e => e.Date.Date == date.Date).ToList();
        }

        public async Task SaveMealEntriesAsync(List<MealEntry> mealEntries)
        {
            var json = JsonSerializer.Serialize(mealEntries, new JsonSerializerOptions 
            { 
                WriteIndented = true,
                Converters = { new DateTimeOffsetJsonConverter() }
            });
            await File.WriteAllTextAsync(_filePath, json);
            _mealEntries = mealEntries;
        }

        public async Task AddMealEntryAsync(MealEntry entry)
        {
            await LoadAllMealEntriesAsync();
            _mealEntries.Add(entry);
            await SaveMealEntriesAsync(_mealEntries);
        }

        public async Task UpdateMealEntryAsync(MealEntry entry)
        {
            await LoadAllMealEntriesAsync();
            var index = _mealEntries.FindIndex(e => e.Id == entry.Id);
            if (index >= 0)
            {
                _mealEntries[index] = entry;
                await SaveMealEntriesAsync(_mealEntries);
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
            _mealEntries = JsonSerializer.Deserialize<List<MealEntry>>(json) ?? new List<MealEntry>();
        }
    }

    public class DateTimeOffsetJsonConverter : System.Text.Json.Serialization.JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return DateTimeOffset.Parse(reader.GetString());
        }

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString("O"));
        }
    }
}
