using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
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
        private readonly string _filePath;
        private List<Product> _products = new List<Product>();
        private readonly JsonSerializerOptions _jsonOptions;

        public event EventHandler ProductsChanged;

        public ProductService()
        {
            var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NutriTrack");
            Directory.CreateDirectory(appDataPath);
            _filePath = Path.Combine(appDataPath, "products.json");
            
            _jsonOptions = new JsonSerializerOptions 
            { 
                WriteIndented = true 
            };
        }

        public async Task<List<Product>> LoadProductsAsync()
        {
            if (!File.Exists(_filePath))
            {
                _products = new List<Product>();
                await SaveProductsAsync(_products);
                return _products;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            _products = JsonSerializer.Deserialize<List<Product>>(json, _jsonOptions) ?? new List<Product>();
            return _products;
        }

        public async Task SaveProductsAsync(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products, _jsonOptions);
            await File.WriteAllTextAsync(_filePath, json);
            _products = products;
            ProductsChanged?.Invoke(this, EventArgs.Empty);
        }

        public async Task AddProductAsync(Product product)
        {
            await LoadProductsAsync(); 
            _products.Add(product);
            await SaveProductsAsync(_products);
        }

        public async Task UpdateProductAsync(Product product)
        {
            await LoadProductsAsync(); 
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
            await LoadProductsAsync(); 
            _products.RemoveAll(p => p.Id == productId);
            await SaveProductsAsync(_products);
        }
    }
}
