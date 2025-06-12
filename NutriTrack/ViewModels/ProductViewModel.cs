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
            set => SetProperty(ref _selectedProduct, value);
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
        }

        private async Task AddProductAsync()
        {
            var newProduct = new Product { Name = "New Product" };
            await _productService.AddProductAsync(newProduct);
            SelectedProduct = newProduct;
        }

        private bool ValidateProduct(Product product)
        {
            if (product == null)
            {
                ErrorMessage = "No product selected.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(product.Name))
            {
                ErrorMessage = "Product name cannot be empty.";
                return false;
            }
            if (Products.Any(p => p.Id != product.Id && p.Name.Trim().ToLower() == product.Name.Trim().ToLower()))
            {
                ErrorMessage = "A product with this name already exists.";
                return false;
            }
            if (product.CaloriesPer100g < 0 || product.Protein < 0 || product.Fat < 0 || product.Carbohydrates < 0)
            {
                ErrorMessage = "Calories, protein, fat, and carbohydrates cannot be negative.";
                return false;
            }
            if (product.Protein + product.Fat + product.Carbohydrates > 100)
            {
                ErrorMessage = "Sum of protein, fat, and carbohydrates cannot exceed 100g per 100g.";
                return false;
            }
            ErrorMessage = null;
            return true;
        }

        private bool CanSaveProduct()
        {
            return ValidateProduct(SelectedProduct);
        }

        private async Task SaveProductAsync()
        {
            if (!ValidateProduct(SelectedProduct))
                return;
            await _productService.UpdateProductAsync(SelectedProduct);
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
