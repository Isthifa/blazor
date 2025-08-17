using Microsoft.AspNetCore.Components.Forms;

namespace BlazorWebPoc.Web.Models
{
    public class ProductCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public IBrowserFile? ImageFile { get; set; }
    }
}
