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

        private DailySummary _summary;
        public DailySummary Summary
        {
            get => _summary;
            set => SetProperty(ref _summary, value);
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
            IProductService productService)
        {
            _dailySummaryService = dailySummaryService;
            _mealEntryService = mealEntryService;
            _productService = productService;

            LoadSummaryCommand = new AsyncRelayCommand(LoadSummaryAsync);

            // Подписываемся на события изменения данных
            _mealEntryService.MealEntriesChanged += async (s, e) => await LoadSummaryAsync();
            _productService.ProductsChanged += async (s, e) => await LoadSummaryAsync();

            _ = LoadSummaryAsync();
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