using BlazorWebPoc.ApiService.Data;
using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.ApiService.Model.Request;
using BlazorWebPoc.ApiService.Service;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace BlazorWebPoc.ApiService.Service.impl
{
    public class ProductServiceImpl : IProductService
    {
        private readonly AppDbContext _dbContext;

        public ProductServiceImpl(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _dbContext.Products.ToListAsync();
        }

        public async Task<Product> GetProductByIdAsync(int id)
        {
            return await _dbContext.Products.FindAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return product;
        }

        public async Task<Product> UpdateProductAsync(int id, Product product)
        {
            var existing = await _dbContext.Products.FindAsync(id);
            if (existing == null) return null;

            existing.Name = product.Name;
            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.UpdatedAt = DateTime.UtcNow;
            if (product.Image != null)
            {
                existing.Image = product.Image;
            }

            await _dbContext.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _dbContext.Products.FindAsync(id);
            if (product == null) return false;

            _dbContext.Products.Remove(product);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<Product> ProductUploadAsync(ProductUploadDto dto)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            byte[]? imageBytes = null;
            if (dto.ImageFile != null)
            {
                using var ms = new MemoryStream();
                await dto.ImageFile.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Image = imageBytes,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _dbContext.Products.Add(product);
            await _dbContext.SaveChangesAsync();

            return product;
        }
    }
}
