using BlazorWebPoc.ApiService.Model;
using Microsoft.AspNetCore.Mvc;
using System.Net;
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

        public async Task<CvrSearchResponse> SearchCvrAsync(string query)
        {
            var response = new CvrSearchResponse();

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(query))
                {
                    response.Message = "Please enter a search query";
                    return response;
                }

                // Clean query - remove spaces and special characters, keep only digits
                var cleanQuery = System.Text.RegularExpressions.Regex.Replace(query.Trim(), @"[^\d]", "");

                if (string.IsNullOrEmpty(cleanQuery))
                {
                    response.Message = "Please enter a valid CVR number (digits only)";
                    return response;
                }

                if (cleanQuery.Length < 3)
                {
                    response.Message = "CVR number must be at least 3 digits long";
                    return response;
                }

                if (cleanQuery.Length > 8)
                {
                    response.Message = "CVR number cannot exceed 8 digits";
                    return response;
                }

                // Rate limiting - could be made configurable
                await Task.Delay(1000);

                var requestUrl = $"{CVR_API_BASE_URL}?search={cleanQuery}&country=dk&format=json";
                _logger.LogInformation("Calling CVR API: {Url}", requestUrl);

                // Set timeout for the request
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

                var httpResponse = await _httpClient.GetAsync(requestUrl, cts.Token);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Message = httpResponse.StatusCode switch
                    {
                        HttpStatusCode.NotFound => "No company found with this CVR number",
                        HttpStatusCode.TooManyRequests => "Too many requests. Please try again later",
                        HttpStatusCode.Unauthorized => "API access denied. Please check your credentials",
                        HttpStatusCode.BadRequest => "Invalid CVR number format",
                        HttpStatusCode.InternalServerError => "CVR service is temporarily unavailable. Please try again later",
                        HttpStatusCode.ServiceUnavailable => "CVR service is currently down for maintenance",
                        _ => $"CVR service error (Status: {httpResponse.StatusCode}). Please try again later"
                    };

                    _logger.LogWarning("CVR API returned status: {Status} for query: {Query}",
                        httpResponse.StatusCode, cleanQuery);
                    return response;
                }

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();

                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    response.Message = "Received empty response from CVR service";
                    _logger.LogWarning("Empty response from CVR API for query: {Query}", cleanQuery);
                    return response;
                }

                _logger.LogDebug("CVR API Response: {Response}", jsonResponse);

                var cvrResponse = JsonSerializer.Deserialize<CvrApiResponse>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                });

                if (cvrResponse?.Vat == null)
                {
                    response.Message = "No company found with this CVR number";
                    _logger.LogInformation("No valid company data found for CVR: {Query}", cleanQuery);
                    return response;
                }

                // Process and clean special characters
                var companyName = ProcessDanishText(cvrResponse.Name ?? "Unknown Company");
                var address = ProcessDanishText(cvrResponse.Address ?? "");
                var city = ProcessDanishText(cvrResponse.City ?? "");

                // Create display text with company info
                var displayText = cvrResponse.Vat.ToString();
                if (!string.IsNullOrEmpty(companyName) && companyName != "Unknown Company")
                {
                    displayText += $" - {companyName}";
                }

                var cvrItem = new CvrAutoCompleteItem
                {
                    Value = cvrResponse.Vat,
                    Text = displayText.ToString(),
                    Data = new CvrSearchResult
                    {
                        Vat = cvrResponse.Vat,
                        Name = companyName,
                        Phone = cvrResponse.Phone ?? "",
                        Email = cvrResponse.Email ?? "",
                        Address = address,
                        City = city,
                        Zipcode = cvrResponse.Zipcode ?? "",
                        Country = cvrResponse.Country ?? "DK"
                    }
                };

                response.Results.Add(cvrItem);
                response.IsSuccess = true;
                response.Message = $"Found {response.Results.Count} result(s)";

                _logger.LogInformation("Successfully found CVR data for: {CVR} - {Company}",
                    cvrResponse.Vat, companyName);
            }
            catch (OperationCanceledException)
            {
                response.Message = "Request timed out. CVR service may be slow or unavailable";
                _logger.LogWarning("CVR API request timed out for query: {Query}", query);
            }
            catch (HttpRequestException ex)
            {
                response.Message = "Unable to connect to CVR service. Please check your internet connection";
                _logger.LogError(ex, "HTTP error calling CVR API for query: {Query}", query);
            }
            catch (JsonException ex)
            {
                response.Message = "Received invalid response from CVR service. Please try again";
                _logger.LogError(ex, "JSON parsing error for CVR API response, query: {Query}", query);
            }
            catch (Exception ex)
            {
                response.Message = "An unexpected error occurred. Please try again later";
                _logger.LogError(ex, "Unexpected error searching CVR numbers for query: {Query}", query);
            }

            return response;
        }

        // Response wrapper class
        public class CvrSearchResponse
        {
            public List<CvrAutoCompleteItem> Results { get; set; } = new();
            public bool IsSuccess { get; set; } = false;
            public string Message { get; set; } = string.Empty;
            public int ResultCount => Results.Count;
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

