namespace BlazorWebPoc.ApiService.Model
{
    public class CvrApiResponse
    {
        public long? Vat { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Zipcode { get; set; }
        public string Country { get; set; } = string.Empty;
        public bool Valid { get; set; }
    }
}
