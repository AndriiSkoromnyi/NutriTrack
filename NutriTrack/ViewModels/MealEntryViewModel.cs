using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;

namespace NutriTrack.ViewModels
{
    public class MealEntryViewModel : ViewModelBase
    {
        private readonly IMealEntryService _mealEntryService;
        private readonly IProductService _productService;

        public ObservableCollection<MealEntryDisplayModel> MealEntries { get; } = new();
        public ObservableCollection<Product> Products { get; } = new();
        
        public ObservableCollection<string> MealTypeNames { get; } = new(
            Enum.GetNames(typeof(MealType))
        );

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
                    }
                    else
                    {
                        SelectedProduct = null;
                        SelectedMealTypeName = null;
                        TimeString = DateTime.Now.ToString("HH:mm");
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

        public MealEntryViewModel(IMealEntryService mealEntryService, IProductService productService)
        {
            _mealEntryService = mealEntryService ?? throw new ArgumentNullException(nameof(mealEntryService));
            _productService = productService ?? throw new ArgumentNullException(nameof(productService));

            LoadMealEntriesCommand = new AsyncRelayCommand(LoadMealEntriesAsync);
            AddMealEntryCommand = new AsyncRelayCommand(AddMealEntryAsync, CanAddMealEntry);
            SaveMealEntryCommand = new AsyncRelayCommand(SaveMealEntryAsync, CanSaveMealEntry);
            DeleteMealEntryCommand = new AsyncRelayCommand(DeleteMealEntryAsync, CanDeleteMealEntry);

            _productService.ProductsChanged += async (s, e) => await LoadProductsAndRefreshEntriesAsync();
            
            _ = LoadProductsAndRefreshEntriesAsync();
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
                    var displayModel = new MealEntryDisplayModel(entry, product);
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
                Weight = 100, 
                MealType = Enum.Parse<MealType>(SelectedMealTypeName),
                Date = selectedDateTime
            };

            await _mealEntryService.AddMealEntryAsync(newEntry);
            await LoadMealEntriesAsync();
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

    public class MealEntryDisplayModel
    {
        private readonly MealEntry _mealEntry;
        private readonly Product _product;

        public MealEntryDisplayModel(MealEntry mealEntry, Product product)
        {
            _mealEntry = mealEntry;
            _product = product;
        }

        public string MealType => _mealEntry.MealType.ToString();
        public string ProductName => _product.Name;
        public double Weight => _mealEntry.Weight;
        public double Calories => Math.Round(_product.CaloriesPer100g * _mealEntry.Weight / 100.0, 1);
        public DateTime Date => _mealEntry.Date;
        public MealEntry MealEntry => _mealEntry;
    }
}
