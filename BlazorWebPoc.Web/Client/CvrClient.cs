using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.Web.Models;
using System.Text.Json;
using static BlazorWebPoc.Web.Components.Pages.CvrAutoCompleteForm;

namespace BlazorWebPoc.Web.Client
{
    public class CvrClient
    {
        private readonly HttpClient _httpClient;

        public CvrClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<CvrAutoCompleteItem>> SearchCvrAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
            {
                return new List<CvrAutoCompleteItem>();
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/Cvr/search?query={Uri.EscapeDataString(query)}", cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);

                    var results = JsonSerializer.Deserialize<List<CvrAutoCompleteItem>>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    return results ?? new List<CvrAutoCompleteItem>();
                }
            }
            catch (OperationCanceledException)
            {
                // Request was cancelled - this is normal behavior
                return new List<CvrAutoCompleteItem>();
            }
            catch (Exception)
            {
                // Log error or handle as needed
                throw;
            }

            return new List<CvrAutoCompleteItem>();
        }
    }
}