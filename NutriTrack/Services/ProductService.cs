using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using NutriTrack.Models;

namespace NutriTrack.Services
{
    public interface IProductService
    {
        Task<List<Product>> LoadProductsAsync();
        Task SaveProductsAsync(List<Product> products);
        Task AddProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(Guid productId);
        event EventHandler ProductsChanged;
    }

    public class ProductService : IProductService
    {
        private bool _isInitialized;
        private readonly object _lockObject = new object();
        private readonly string _filePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private List<Product> _products;

        public event EventHandler ProductsChanged;

        public ProductService()
        {
            var dataPath = DataPathHelper.GetDataPath();
            _filePath = Path.Combine(dataPath, "products.json");

            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true
            };
            
            _products = new List<Product>();
            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            if (_isInitialized) return;

            lock (_lockObject)
            {
                if (_isInitialized) return;
                
                try
                {
                    if (File.Exists(_filePath))
                    {
                        var json = File.ReadAllText(_filePath);
                        _products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
                        ProductsChanged?.Invoke(this, EventArgs.Empty);
                    }
                    else
                    {
                        _products = new List<Product>();
                        var json = JsonSerializer.Serialize(_products, _jsonOptions);
                        File.WriteAllText(_filePath, json);
                    }
                }
                catch (Exception ex)
                {
                    _products = new List<Product>();
                    Console.WriteLine($"Error initializing products: {ex.Message}");
                }

                _isInitialized = true;
            }
        }

        public async Task<List<Product>> LoadProductsAsync()
        {
            await InitializeAsync();
            return new List<Product>(_products);
        }

        public async Task SaveProductsAsync(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            _products = new List<Product>(products);
            _isInitialized = true;
            ProductsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddProductAsync(Product product)
        {
            await InitializeAsync();
            _products.Add(product);
            await SaveProductsAsync(_products);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await InitializeAsync();
            var index = _products.FindIndex(p => p.Id == product.Id);
            if (index >= 0)
            {
                _products[index] = product;
                await SaveProductsAsync(_products);
            }
            else
            {
                throw new ArgumentException("Product not found");
            }
        }

        public async Task DeleteProductAsync(Guid productId)
        {
            await InitializeAsync();
            _products.RemoveAll(p => p.Id == productId);
            await SaveProductsAsync(_products);
        }
    }
}
