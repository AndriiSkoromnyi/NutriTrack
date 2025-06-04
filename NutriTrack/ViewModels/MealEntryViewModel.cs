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

        public ObservableCollection<MealEntry> MealEntries { get; } = new ObservableCollection<MealEntry>();
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
            foreach (var e in entries)
                MealEntries.Add(e);
        }

        private async Task AddMealEntryAsync()
        {
            if (Products.Count == 0)
                return;

            var newEntry = new MealEntry
            {
                ProductId = Products[0].Id,
                Weight = 100,
                MealType = MealType.Breakfast,
                Date = SelectedDate
            };
            await _mealEntryService.AddMealEntryAsync(newEntry);
            MealEntries.Add(newEntry);
            SelectedMealEntry = newEntry;
        }

        private async Task SaveMealEntryAsync()
        {
            if (SelectedMealEntry != null)
                await _mealEntryService.UpdateMealEntryAsync(SelectedMealEntry);
        }

        private async Task DeleteMealEntryAsync()
        {
            if (SelectedMealEntry != null)
            {
                await _mealEntryService.DeleteMealEntryAsync(SelectedMealEntry.Id);
                MealEntries.Remove(SelectedMealEntry);
                SelectedMealEntry = null;
            }
        }
    }
}
