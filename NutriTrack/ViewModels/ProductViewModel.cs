using System.Collections.ObjectModel;
using System.Threading.Tasks;
using NutriTrack.Models;
using NutriTrack.Services;
using CommunityToolkit.Mvvm.Input;

namespace NutriTrack.ViewModels
{
    public class ProductViewModel : ViewModelBase
    {
        private readonly IProductService _productService;
        private readonly IUserSettingsService _userSettingsService;
        private readonly IWeightConversionService _weightConversionService;
        private WeightUnit _currentWeightUnit = WeightUnit.Grams;

        public string WeightUnitDisplay => _currentWeightUnit == WeightUnit.Grams ? "g" : "oz";

        public ObservableCollection<Product> Products { get; } = new ObservableCollection<Product>();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set => SetProperty(ref _selectedProduct, value);
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
            SaveProductCommand = new AsyncRelayCommand(SaveProductAsync);
            DeleteProductCommand = new AsyncRelayCommand(DeleteProductAsync);

            _userSettingsService.SettingsChanged += async (s, e) => await LoadUserSettingsAsync();

            // Load initial settings and products
            _ = LoadUserSettingsAsync();
            _ = LoadProductsAsync();
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
            Products.Clear();
            var products = await _productService.LoadProductsAsync();
            foreach (var p in products)
                Products.Add(p);
        }

        private async Task AddProductAsync()
        {
            var newProduct = new Product { Name = "New Product" };
            await _productService.AddProductAsync(newProduct);
            Products.Add(newProduct);
            SelectedProduct = newProduct;
        }

        private async Task SaveProductAsync()
        {
            if (SelectedProduct != null)
                await _productService.UpdateProductAsync(SelectedProduct);
        }

        private async Task DeleteProductAsync()
        {
            if (SelectedProduct != null)
            {
                await _productService.DeleteProductAsync(SelectedProduct.Id);
                Products.Remove(SelectedProduct);
                SelectedProduct = null;
            }
        }
    }
}
