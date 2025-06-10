using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;

namespace NutriTrack.ViewModels
{
    public class MealEntryViewModel : ViewModelBase
    {
        private readonly IMealEntryService _mealEntryService;
        private readonly IProductService _productService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IWeightConversionService _weightConversionService;

        private WeightUnit _currentWeightUnit = WeightUnit.Grams;

        public ObservableCollection<MealEntryDisplayModel> MealEntries { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        
        public ObservableCollection<string> MealTypeNames { get; } = new(
            Enum.GetNames(typeof(MealType))
        );

        public string WeightUnitDisplay => _currentWeightUnit == WeightUnit.Grams ? "g" : "oz";

        private double _displayWeight = 100;
        public double DisplayWeight
        {
            get => _displayWeight;
            set
            {
                if (SetProperty(ref _displayWeight, value))
                {
                    // Convert display weight (in current unit) to grams for storage
                    Weight = _weightConversionService.Convert(value, _currentWeightUnit, WeightUnit.Grams);
                }
            }
        }

        private MealEntryDisplayModel _selectedMealEntryDisplay;
        public MealEntryDisplayModel SelectedMealEntryDisplay
        {
            get => _selectedMealEntryDisplay;
            set
            {
                if (SetProperty(ref _selectedMealEntryDisplay, value))
                {
                    SelectedMealEntry = value?.MealEntry;
                    if (value?.MealEntry != null)
                    {
                        TimeString = value.MealEntry.Date.ToString("HH:mm");
                    }
                    SaveMealEntryCommand.NotifyCanExecuteChanged();
                    DeleteMealEntryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private MealEntry _selectedMealEntry;
        public MealEntry SelectedMealEntry
        {
            get => _selectedMealEntry;
            set
            {
                if (SetProperty(ref _selectedMealEntry, value))
                {
                    if (value != null)
                    {
                        SelectedProduct = Products.FirstOrDefault(p => p.Id == value.ProductId);
                        SelectedMealTypeName = value.MealType.ToString();
                        TimeString = value.Date.ToString("HH:mm");
                        Weight = value.Weight;
                    }
                    else
                    {
                        SelectedProduct = null;
                        SelectedMealTypeName = null;
                        TimeString = DateTime.Now.ToString("HH:mm");
                        Weight = 100;
                    }
                }
            }
        }

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    AddMealEntryCommand.NotifyCanExecuteChanged();
                    SaveMealEntryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _selectedMealTypeName;
        public string SelectedMealTypeName
        {
            get => _selectedMealTypeName;
            set
            {
                if (SetProperty(ref _selectedMealTypeName, value))
                {
                    AddMealEntryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private DateTimeOffset _selectedDate = DateTimeOffset.Now;
        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value))
                {
                    _ = LoadMealEntriesAsync();
                }
            }
        }

        private string _timeString = DateTime.Now.ToString("HH:mm");
        public string TimeString
        {
            get => _timeString;
            set => SetProperty(ref _timeString, value);
        }

        private double _weight = 100;
        public double Weight
        {
            get => _weight;
            set
            {
                if (SetProperty(ref _weight, value))
                {
                    if (SelectedMealEntry != null)
                    {
                        SelectedMealEntry.Weight = value;
                    }
                    AddMealEntryCommand.NotifyCanExecuteChanged();
                    SaveMealEntryCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private TimeSpan ParseTimeString(string timeString)
        {
            if (TimeSpan.TryParse(timeString, out TimeSpan result))
            {
                return result;
            }
            return DateTime.Now.TimeOfDay;
        }

        public IAsyncRelayCommand LoadMealEntriesCommand { get; }
        public IAsyncRelayCommand AddMealEntryCommand { get; }
        public IAsyncRelayCommand SaveMealEntryCommand { get; }
        public IAsyncRelayCommand DeleteMealEntryCommand { get; }

        public MealEntryViewModel(
            IMealEntryService mealEntryService,
            IProductService productService,
            IUserSettingsService userSettingsService,
            IWeightConversionService weightConversionService)
        {
            _mealEntryService = mealEntryService ?? throw new ArgumentNullException(nameof(mealEntryService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));
            _userSettingsService = userSettingsService ?? throw new ArgumentNullException(nameof(userSettingsService));
            _weightConversionService = weightConversionService ?? throw new ArgumentNullException(nameof(weightConversionService));

            LoadMealEntriesCommand = new AsyncRelayCommand(LoadMealEntriesAsync);
            AddMealEntryCommand = new AsyncRelayCommand(AddMealEntryAsync, CanAddMealEntry);
            SaveMealEntryCommand = new AsyncRelayCommand(SaveMealEntryAsync, CanSaveMealEntry);
            DeleteMealEntryCommand = new AsyncRelayCommand(DeleteMealEntryAsync, CanDeleteMealEntry);

            _productService.ProductsChanged += async (s, e) => await LoadProductsAndRefreshEntriesAsync();
            _userSettingsService.SettingsChanged += async (s, e) => await LoadUserSettingsAsync();
            
            _ = LoadProductsAndRefreshEntriesAsync();
            _ = LoadUserSettingsAsync();
        }

        private async Task LoadUserSettingsAsync()
        {
            var settings = await _userSettingsService.LoadSettingsAsync();
            if (settings != null && settings.WeightUnit != _currentWeightUnit)
            {
                _currentWeightUnit = settings.WeightUnit;
                
                // Update all UI elements that depend on the weight unit
                OnPropertyChanged(nameof(WeightUnitDisplay));
                
                // Convert and update display weight
                _displayWeight = _weightConversionService.Convert(Weight, WeightUnit.Grams, _currentWeightUnit);
                OnPropertyChanged(nameof(DisplayWeight));
                
                // Force UI refresh for all bound properties
                foreach (var entry in MealEntries)
                {
                    entry.RefreshDisplay(_weightConversionService, _currentWeightUnit);
                }
                
                // Refresh the collection to trigger UI update
                var tempList = new List<MealEntryDisplayModel>(MealEntries);
                MealEntries.Clear();
                foreach (var entry in tempList)
                {
                    MealEntries.Add(entry);
                }
            }
        }

        private bool CanAddMealEntry()
        {
            return SelectedProduct != null && !string.IsNullOrEmpty(SelectedMealTypeName);
        }

        private bool CanSaveMealEntry()
        {
            return SelectedMealEntryDisplay != null && SelectedProduct != null;
        }

        private bool CanDeleteMealEntry()
        {
            return SelectedMealEntryDisplay != null;
        }

        private async Task LoadProductsAndRefreshEntriesAsync()
        {
            await LoadProductsAsync();
            await LoadMealEntriesAsync();
        }

        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var products = await _productService.LoadProductsAsync();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            
            if (SelectedProduct != null && !products.Any(p => p.Id == SelectedProduct.Id))
            {
                SelectedProduct = null;
            }
        }

        private async Task LoadMealEntriesAsync()
        {
            MealEntries.Clear();
            var entries = await _mealEntryService.LoadMealEntriesAsync(SelectedDate.DateTime);
            var products = await _productService.LoadProductsAsync();

            foreach (var entry in entries.OrderBy(e => e.Date))
            {
                var product = products.FirstOrDefault(p => p.Id == entry.ProductId);
                if (product != null) 
                {
                    var displayModel = new MealEntryDisplayModel(entry, product, _weightConversionService, _currentWeightUnit);
                    MealEntries.Add(displayModel);
                }
            }
        }

        private async Task AddMealEntryAsync()
        {
            if (!CanAddMealEntry())
                return;

            var time = ParseTimeString(TimeString);
            var selectedDateTime = SelectedDate.DateTime.Date.Add(time);

            var newEntry = new MealEntry
            {
                ProductId = SelectedProduct.Id,
                Weight = Weight,
                MealType = Enum.Parse<MealType>(SelectedMealTypeName),
                Date = selectedDateTime
            };

            await _mealEntryService.AddMealEntryAsync(newEntry);
            await LoadMealEntriesAsync();
            
            // Update the selected entry after adding
            SelectedMealEntry = newEntry;
        }

        private async Task SaveMealEntryAsync()
        {
            if (!CanSaveMealEntry())
                return;

            SelectedMealEntry.ProductId = SelectedProduct.Id;
            if (Enum.TryParse<MealType>(SelectedMealTypeName, out var mealType))
            {
                SelectedMealEntry.MealType = mealType;
            }
            
            SelectedMealEntry.Weight = Weight;
            
            var time = ParseTimeString(TimeString);
            var newDateTime = SelectedMealEntry.Date.Date.Add(time);
            SelectedMealEntry.Date = newDateTime;

            await _mealEntryService.UpdateMealEntryAsync(SelectedMealEntry);
            await LoadMealEntriesAsync();
        }

        private async Task DeleteMealEntryAsync()
        {
            if (!CanDeleteMealEntry())
                return;

            await _mealEntryService.DeleteMealEntryAsync(SelectedMealEntryDisplay.MealEntry.Id);
            await LoadMealEntriesAsync();
            SelectedMealEntry = null;
            SelectedMealEntryDisplay = null;
        }
    }

    public class MealEntryDisplayModel : ViewModelBase
    {
        private readonly MealEntry _mealEntry;
        private readonly Product _product;
        private readonly IWeightConversionService _weightConversionService;
        private WeightUnit _displayUnit;
        private double _weight;
        private string _weightDisplay;

        public MealEntryDisplayModel(
            MealEntry mealEntry,
            Product product,
            IWeightConversionService weightConversionService,
            WeightUnit displayUnit)
        {
            _mealEntry = mealEntry;
            _product = product;
            _weightConversionService = weightConversionService;
            _displayUnit = displayUnit;
            _weight = _weightConversionService.Convert(_mealEntry.Weight, WeightUnit.Grams, _displayUnit);
            _weightDisplay = _weightConversionService.FormatWeight(_weight, _displayUnit);
        }

        public string MealType => _mealEntry.MealType.ToString();
        public string ProductName => _product.Name;
        public double Weight
        {
            get => _weight;
            private set => SetProperty(ref _weight, value);
        }
        public string WeightDisplay
        {
            get => _weightDisplay;
            private set => SetProperty(ref _weightDisplay, value);
        }
        public double Calories => Math.Round(_product.CaloriesPer100g * _mealEntry.Weight / 100.0, 1);
        public DateTime Date => _mealEntry.Date;
        public MealEntry MealEntry => _mealEntry;

        public void RefreshDisplay(IWeightConversionService weightConversionService, WeightUnit displayUnit)
        {
            _displayUnit = displayUnit;
            Weight = weightConversionService.Convert(_mealEntry.Weight, WeightUnit.Grams, displayUnit);
            WeightDisplay = weightConversionService.FormatWeight(Weight, displayUnit);
        }
    }
}
