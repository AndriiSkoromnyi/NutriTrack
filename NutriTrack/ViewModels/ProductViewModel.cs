using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;
using System.Linq;

namespace NutriTrack.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IWeightConversionService _weightConversionService;
        private WeightUnit _currentWeightUnit = WeightUnit.Grams;
        private string _errorMessage;

        public string WeightUnitDisplay => _currentWeightUnit == WeightUnit.Grams ? "g" : "oz";

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set
            {
                if (SetProperty(ref _selectedProduct, value))
                {
                    if (value != null)
                    {
                        EditName = value.Name;
                        EditCaloriesPer100g = value.CaloriesPer100g;
                        EditProtein = value.Protein;
                        EditFat = value.Fat;
                        EditCarbohydrates = value.Carbohydrates;
                    }
                }
            }
        }

        // Temporary editable fields
        private string _editName;
        public string EditName
        {
            get => _editName;
            set
            {
                if (SetProperty(ref _editName, value))
                    SaveProductCommand.NotifyCanExecuteChanged();
            }
        }
        private double _editCaloriesPer100g;
        public double EditCaloriesPer100g
        {
            get => _editCaloriesPer100g;
            set
            {
                if (SetProperty(ref _editCaloriesPer100g, value))
                    SaveProductCommand.NotifyCanExecuteChanged();
            }
        }
        private double _editProtein;
        public double EditProtein
        {
            get => _editProtein;
            set
            {
                if (SetProperty(ref _editProtein, value))
                    SaveProductCommand.NotifyCanExecuteChanged();
            }
        }
        private double _editFat;
        public double EditFat
        {
            get => _editFat;
            set
            {
                if (SetProperty(ref _editFat, value))
                    SaveProductCommand.NotifyCanExecuteChanged();
            }
        }
        private double _editCarbohydrates;
        public double EditCarbohydrates
        {
            get => _editCarbohydrates;
            set
            {
                if (SetProperty(ref _editCarbohydrates, value))
                    SaveProductCommand.NotifyCanExecuteChanged();
            }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public IAsyncRelayCommand LoadProductsCommand { get; }
        public IAsyncRelayCommand AddProductCommand { get; }
        public IAsyncRelayCommand SaveProductCommand { get; }
        public IAsyncRelayCommand DeleteProductCommand { get; }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    UpdateFilteredProducts();
                }
            }
        }

        public ObservableCollection<Product> FilteredProducts { get; } = new ObservableCollection<Product>();

        private void UpdateFilteredProducts()
        {
            FilteredProducts.Clear();
            var filtered = string.IsNullOrWhiteSpace(SearchText)
                ? Products
                : Products.Where(p => p.Name != null && p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase));
            foreach (var product in filtered)
                FilteredProducts.Add(product);
        }

        public ProductViewModel(
            IProductService productService,
            IUserSettingsService userSettingsService,
            IWeightConversionService weightConversionService)
        {
            _productService = productService;
            _userSettingsService = userSettingsService;
            _weightConversionService = weightConversionService;

            LoadProductsCommand = new AsyncRelayCommand(LoadProductsAsync);
            AddProductCommand = new AsyncRelayCommand(AddProductAsync);
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync, CanSaveProduct);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync);

            _userSettingsService.SettingsChanged += async (s, e) => await LoadUserSettingsAsync();
            _productService.ProductsChanged += async (s, e) => await LoadProductsAsync();

            // Load initial settings and products
            LoadInitialDataAsync().ConfigureAwait(false);
        }

        private async Task LoadInitialDataAsync()
        {
            await LoadUserSettingsAsync();
            await LoadProductsAsync();
        }

        private async Task LoadUserSettingsAsync()
        {
            var settings = await _userSettingsService.LoadSettingsAsync();
            if (settings != null && settings.WeightUnit != _currentWeightUnit)
            {
                _currentWeightUnit = settings.WeightUnit;
                OnPropertyChanged(nameof(WeightUnitDisplay));
            }
        }

        private async Task LoadProductsAsync()
        {
            var products = await _productService.LoadProductsAsync();
            
            Products.Clear();
            foreach (var product in products)
            {
                Products.Add(product);
            }
            UpdateFilteredProducts();
        }

        private async Task AddProductAsync()
        {
            var newProduct = new Product { Name = "New Product" };
            await _productService.AddProductAsync(newProduct);
            SelectedProduct = newProduct;
        }

        private bool ValidateProductFields()
        {
            if (string.IsNullOrWhiteSpace(EditName))
            {
                ErrorMessage = "Product name cannot be empty.";
                return false;
            }
            if (Products.Any(p => p.Id != SelectedProduct?.Id && p.Name.Trim().ToLower() == EditName.Trim().ToLower()))
            {
                ErrorMessage = "A product with this name already exists.";
                return false;
            }
            if (EditCaloriesPer100g < 0 || EditProtein < 0 || EditFat < 0 || EditCarbohydrates < 0)
            {
                ErrorMessage = "Calories, protein, fat, and carbohydrates cannot be negative.";
                return false;
            }
            if (EditProtein + EditFat + EditCarbohydrates > 100)
            {
                ErrorMessage = "Sum of protein, fat, and carbohydrates cannot exceed 100g per 100g.";
                return false;
            }
            ErrorMessage = null;
            return true;
        }

        private bool CanSaveProduct()
        {
            return SelectedProduct != null && ValidateProductFields();
        }

        private async Task SaveProductAsync()
        {
            if (SelectedProduct == null) return;
            if (!ValidateProductFields()) return;
            SelectedProduct.Name = EditName;
            SelectedProduct.CaloriesPer100g = EditCaloriesPer100g;
            SelectedProduct.Protein = EditProtein;
            SelectedProduct.Fat = EditFat;
            SelectedProduct.Carbohydrates = EditCarbohydrates;
            var id = SelectedProduct.Id;
            await _productService.UpdateProductAsync(SelectedProduct);
            await LoadProductsAsync();
            var updated = Products.FirstOrDefault(p => p.Id == id);
            if (updated != null)
            {
                SelectedProduct = updated;
                EditName = updated.Name;
                EditCaloriesPer100g = updated.CaloriesPer100g;
                EditProtein = updated.Protein;
                EditFat = updated.Fat;
                EditCarbohydrates = updated.Carbohydrates;
            }
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct != null)
            {
                await _productService.DeleteProductAsync(SelectedProduct.Id);
                SelectedProduct = null;
            }
        }
    }
}
