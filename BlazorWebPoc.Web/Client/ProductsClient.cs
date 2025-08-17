using BlazorWebPoc.Web.Client.Services;
using BlazorWebPoc.Web.Models;

public class ProductsClient
{
    private readonly HttpClient _httpClient;
    private readonly CustomAuthStateProvider _authProvider;

    public ProductsClient(HttpClient httpClient, CustomAuthStateProvider authProvider)
    {
        _httpClient = httpClient;
        _authProvider = authProvider;
    }

    private async Task AddAuthHeaderAsync()
    {
        var token = _authProvider.GetToken();
        _httpClient.DefaultRequestHeaders.Authorization =
            string.IsNullOrWhiteSpace(token) ? null : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<Product>> GetProducts()
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync("api/Product/getAll");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<List<Product>>() ?? new List<Product>();
        }

        throw new HttpRequestException($"Failed to load products: {response.StatusCode}");
    }

    public async Task<Product?> GetProductById(int id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.GetAsync($"api/products/{id}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Product>();
        }

        return null;
    }

    public async Task<Product> CreateProductAsync(ProductCreateRequest request)
    {
        await AddAuthHeaderAsync();

        using var formData = new MultipartFormDataContent();
        formData.Add(new StringContent(request.Name), "Name");
        formData.Add(new StringContent(request.Description), "Description");
        formData.Add(new StringContent(request.Price.ToString()), "Price");

        if (request.ImageFile != null)
        {
            var imageContent = new StreamContent(request.ImageFile.OpenReadStream());
            imageContent.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(request.ImageFile.ContentType);
            formData.Add(imageContent, "ImageFile", request.ImageFile.Name);
        }

        var response = await _httpClient.PostAsync("api/products/upload", formData);

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<Product>()
                   ?? throw new Exception("Failed to deserialize product");
        }

        var error = await response.Content.ReadAsStringAsync();
        throw new HttpRequestException($"Error: {response.StatusCode} - {error}");
    }

    public async Task<bool> DeleteProductAsync(int id)
    {
        await AddAuthHeaderAsync();
        var response = await _httpClient.DeleteAsync($"api/products/{id}");
        return response.IsSuccessStatusCode;
    }
}
