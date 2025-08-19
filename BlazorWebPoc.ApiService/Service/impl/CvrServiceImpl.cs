using BlazorWebPoc.ApiService.Model;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlazorWebPoc.ApiService.Service.impl
{
    // Models/CvrModels.cs
    public class CvrServiceImpl : ICvrService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvrServiceImpl> _logger;
        private const string CVR_API_BASE_URL = "https://cvrapi.dk/api";

        public CvrServiceImpl(HttpClient httpClient, ILogger<CvrServiceImpl> logger)
        {
            _httpClient = httpClient;
            _logger = logger;

            // Set user agent and encoding to handle Unicode properly
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "CVR-Lookup-App/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8");
        }

        public async Task<List<CvrAutoCompleteItem>> SearchCvrAsync(string query)
        {
            var results = new List<CvrAutoCompleteItem>();

            try
            {
                // Add delay to respect rate limit (1 req/sec for free tier)
                await Task.Delay(1000);

                // Clean query - remove spaces and special characters
                var cleanQuery = System.Text.RegularExpressions.Regex.Replace(query, @"[^\d]", "");

                if (string.IsNullOrEmpty(cleanQuery) || cleanQuery.Length < 3)
                    return results;

                var requestUrl = $"{CVR_API_BASE_URL}?search={cleanQuery}&country=dk&format=json";
                _logger.LogInformation("Calling CVR API: {Url}", requestUrl);

                var response = await _httpClient.GetAsync(requestUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Ensure we read the response with UTF-8 encoding
                    //var responseBytes = await response.Content.ReadAsByteArrayAsync();
                    //var jsonResponse = System.Text.Encoding.UTF8.GetString(responseBytes);
                    var jsonResponse = await response.Content.ReadAsStringAsync();

                    _logger.LogInformation("CVR API Response: {Response}", jsonResponse);

                    var cvrResponse = JsonSerializer.Deserialize<CvrApiResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    if (cvrResponse != null && cvrResponse.Vat!=null)
                    {
                        // Process and clean special characters
                        var companyName = ProcessDanishText(cvrResponse.Name ?? "");
                        var address = ProcessDanishText(cvrResponse.Address ?? "");
                        var city = ProcessDanishText(cvrResponse.City ?? "");

                        // Create display text with company info
                        var displayText = $"{cvrResponse.Vat}";
                        if (!string.IsNullOrEmpty(companyName))
                            displayText += $" - {companyName}";

                        results.Add(new CvrAutoCompleteItem
                        {
                            Value = cvrResponse.Vat,
                            Text = displayText,
                            Data = new CvrSearchResult
                            {
                                Vat = cvrResponse.Vat,
                                Name = companyName,
                                Phone = cvrResponse.Phone ?? "",
                                Email = cvrResponse.Email ?? "",
                                Address = address,
                                City = city,
                                Zipcode = cvrResponse.Zipcode,
                                Country = cvrResponse.Country
                            }
                        });
                    }
                }
                else
                {
                    _logger.LogWarning("CVR API returned status: {Status}, Response: {Response}",
                        response.StatusCode, await response.Content.ReadAsStringAsync());
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error calling CVR API for query: {Query}", query);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON parsing error for CVR API response, query: {Query}", query);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error searching CVR numbers for query: {Query}", query);
            }

            return results;
        }

        private static string ProcessDanishText(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            try
            {
                // Method 1: Handle Unicode escape sequences
                var unescaped = System.Text.RegularExpressions.Regex.Unescape(input);

                // Method 2: Handle HTML entities
                var decoded = System.Net.WebUtility.HtmlDecode(unescaped);

                // Method 3: Handle specific Danish character mappings if needed
                decoded = decoded
                    .Replace("\\u00e6", "æ")  // æ
                    .Replace("\\u00f8", "ø")  // ø  
                    .Replace("\\u00e5", "å")  // å
                    .Replace("\\u00c6", "Æ")  // Æ
                    .Replace("\\u00d8", "Ø")  // Ø
                    .Replace("\\u00c5", "Å")  // Å
                    .Replace("\\u00e9", "é")  // é
                    .Replace("\\u00fc", "ü")  // ü
                    .Replace("\\u00f6", "ö")  // ö
                    .Replace("\\u00e4", "ä"); // ä

                return decoded.Trim();
            }
            catch (Exception)
            {
                return input; // Return original if processing fails
            }
        }

        public async Task<CvrSearchResult?> GetCvrDetailsAsync(string cvrNumber)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{CVR_API_BASE_URL}?search={cvrNumber}&country=dk&format=json");

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var cvrResponse = JsonSerializer.Deserialize<CvrApiResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (cvrResponse != null && cvrResponse.Valid)
                    {
                        return new CvrSearchResult
                        {
                            Vat = cvrResponse.Vat,
                            Name = cvrResponse.Name,
                            Phone = cvrResponse.Phone,
                            Email = cvrResponse.Email,
                            Address = cvrResponse.Address,
                            City = cvrResponse.City,
                            Zipcode = cvrResponse.Zipcode,
                            Country = cvrResponse.Country
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting CVR details for number: {CvrNumber}", cvrNumber);
            }

            return null;
        }
    }
}

