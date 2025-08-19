namespace BlazorWebPoc.ApiService.Model
{
    public class CvrSearchResult
    {
        private string _name = string.Empty;
        private string _address = string.Empty;
        private string _city = string.Empty;

        public long? Vat { get; set; } 

        public string Name
        {
            get => _name;
            set => _name = DecodeUnicodeString(value ?? string.Empty);
        }

        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        public string Address
        {
            get => _address;
            set => _address = DecodeUnicodeString(value ?? string.Empty);
        }

        public string City
        {
            get => _city;
            set => _city = DecodeUnicodeString(value ?? string.Empty);
        }

        public string? Zipcode { get; set; }
        public string Country { get; set; } = string.Empty;

        private static string DecodeUnicodeString(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            try
            {
                // Handle Unicode escape sequences like \u00e6 for æ
                return System.Text.RegularExpressions.Regex.Unescape(input);
            }
            catch
            {
                return input; // Return original if decoding fails
            }
        }
    }
}
