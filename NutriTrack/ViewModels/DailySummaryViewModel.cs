using System;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;

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

        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                    _ = LoadSummaryAsync();
            }
        }

        public IAsyncRelayCommand LoadSummaryCommand { get; }

        public DailySummaryViewModel(IDailySummaryService dailySummaryService,
            IMealEntryService mealEntryService,
            IProductService productService)
        {
            _dailySummaryService = dailySummaryService;
            _mealEntryService = mealEntryService;
            _productService = productService;

            LoadSummaryCommand = new AsyncRelayCommand(LoadSummaryAsync);

            _ = LoadSummaryAsync();
        }

        private async Task LoadSummaryAsync()
        {
            var mealEntries = await _mealEntryService.LoadMealEntriesAsync(SelectedDate);
            var products = await _productService.LoadProductsAsync();

            Summary = await _dailySummaryService.CalculateDailySummaryAsync(SelectedDate, mealEntries, products);
        }
    }
}