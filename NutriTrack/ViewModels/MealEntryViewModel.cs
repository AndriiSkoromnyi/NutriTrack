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

        public ObservableCollection<MealEntryDisplayModel> MealEntries { get; } = new ObservableCollection<MealEntryDisplayModel>();
        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        // Коллекция строковых названий MealType для ComboBox
        public ObservableCollection<string> MealTypeNames { get; } = new ObservableCollection<string>(
            Enum.GetNames(typeof(MealType))
        );

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
                if (SetProperty(ref _selectedProduct, value) && SelectedMealEntry != null && value != null)
                {
                    SelectedMealEntry.ProductId = value.Id;
                }
            }
        }

        private string _selectedMealTypeName;
        public string SelectedMealTypeName
        {
            get => _selectedMealTypeName;
            set
            {
                if (SetProperty(ref _selectedMealTypeName, value) && SelectedMealEntry != null)
                {
                    if (Enum.TryParse<MealType>(value, out var parsed))
                    {
                        SelectedMealEntry.MealType = parsed;
                    }
                }
            }
        }

        private DateTimeOffset _selectedDate = DateTimeOffset.Now.Date;
        public DateTimeOffset SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (SetProperty(ref _selectedDate, value.Date))
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
            AddMealEntryCommand = new AsyncRelayCommand(AddMealEntryAsync);
            SaveMealEntryCommand = new AsyncRelayCommand(SaveMealEntryAsync);
            DeleteMealEntryCommand = new AsyncRelayCommand(DeleteMealEntryAsync);

            _ = LoadProductsAsync();
            _ = LoadMealEntriesAsync();
        }

        private async Task LoadProductsAsync()
        {
            Products.Clear();
            var products = await _productService.LoadProductsAsync();
            foreach (var p in products)
                Products.Add(p);
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
            if (Products.Count == 0)
                return;

            var defaultTime = MealType.Breakfast switch
            {
                MealType.Breakfast => new TimeSpan(8, 0, 0),
                MealType.Lunch => new TimeSpan(13, 0, 0),
                MealType.Dinner => new TimeSpan(19, 0, 0),
                _ => new TimeSpan(11, 0, 0) // Snack
            };

            var newEntry = new MealEntry
            {
                ProductId = Products[0].Id,
                Weight = 100,
                MealType = MealType.Breakfast,
                Date = SelectedDate.DateTime.Add(defaultTime)
            };

            await _mealEntryService.AddMealEntryAsync(newEntry);
            await LoadMealEntriesAsync();
            SelectedMealEntry = newEntry;
        }

        private async Task SaveMealEntryAsync()
        {
            if (SelectedMealEntry != null)
            {
                await _mealEntryService.UpdateMealEntryAsync(SelectedMealEntry);
                await LoadMealEntriesAsync();
            }
        }

        private async Task DeleteMealEntryAsync()
        {
            if (SelectedMealEntry != null)
            {
                await _mealEntryService.DeleteMealEntryAsync(SelectedMealEntry.Id);
                await LoadMealEntriesAsync();
                SelectedMealEntry = null;
            }
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
