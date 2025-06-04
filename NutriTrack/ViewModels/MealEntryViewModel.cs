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

        // Коллекция строковых названий MealType для ComboBox
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
                    }
                    else
                    {
                        SelectedProduct = null;
                        SelectedMealTypeName = null;
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

            // Если выбранный продукт был удален, очищаем выбор
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
                if (product != null) // Показываем только записи с существующими продуктами
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

            var currentTime = DateTime.Now.TimeOfDay;
            var selectedDateTime = SelectedDate.DateTime.Date.Add(currentTime);
            
            // If the resulting date time would be in the future, use the selected date with current time
            if (selectedDateTime > DateTime.Now)
            {
                selectedDateTime = SelectedDate.DateTime.Date;
            }

            var newEntry = new MealEntry
            {
                ProductId = SelectedProduct.Id,
                Weight = 100, // Можно добавить свойство для веса по умолчанию
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
