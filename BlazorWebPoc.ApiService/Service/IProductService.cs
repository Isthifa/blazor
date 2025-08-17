using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.ApiService.Model.Request;

namespace BlazorWebPoc.ApiService.Service
{
    public interface IProductService
    {
        Task<List<Product>> GetAllProductsAsync();
        Task<Product> GetProductByIdAsync(int id);
        Task<Product> CreateProductAsync(Product product);
        Task<Product> UpdateProductAsync(int id,Product product);
        Task<bool> DeleteProductAsync(int id);

        Task<Product> ProductUploadAsync(ProductUploadDto productUploadDto);
    }
}
