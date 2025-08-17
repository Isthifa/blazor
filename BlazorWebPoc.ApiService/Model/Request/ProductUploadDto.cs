using System.Text.Json.Serialization;

namespace BlazorWebPoc.ApiService.Model.Request
{
    public class ProductUploadDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        [JsonIgnore]
        public IFormFile? ImageFile { get; set; }  
    }
}
