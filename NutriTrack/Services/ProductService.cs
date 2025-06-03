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
    }

    public class ProductService : IProductService
    {
        private readonly string _filePath = "products.json";
        private List<Product> _products = new List<Product>();

        public async Task<List<Product>> LoadProductsAsync()
        {
            if (!File.Exists(_filePath))
            {
                _products = new List<Product>();
                await SaveProductsAsync(_products);
                return _products;
            }

            var json = await File.ReadAllTextAsync(_filePath);
            _products = JsonSerializer.Deserialize<List<Product>>(json) ?? new List<Product>();
            return _products;
        }

        public async Task SaveProductsAsync(List<Product> products)
        {
            var json = JsonSerializer.Serialize(products, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(_filePath, json);
            _products = products;
        }

        public async Task AddProductAsync(Product product)
        {
            _products.Add(product);
            await SaveProductsAsync(_products);
        }

        public async Task UpdateProductAsync(Product product)
        {
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
            _products.RemoveAll(p => p.Id == productId);
            await SaveProductsAsync(_products);
        }
    }
}
