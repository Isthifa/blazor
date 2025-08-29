using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.Web.Models;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;
using static BlazorWebPoc.Web.Components.Pages.CvrAutoCompleteForm;

namespace BlazorWebPoc.Web.Client
{
    public class CvrClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CvrClient> _logger;

        public CvrClient(HttpClient httpClient, ILogger<CvrClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public async Task<CvrClientResponse> SearchCvrAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = new CvrClientResponse();

            try
            {
                // Input validation
                if (string.IsNullOrWhiteSpace(query))
                {
                    response.Message = "Please enter a search query";
                    return response;
                }

                var trimmedQuery = query.Trim();
                if (trimmedQuery.Length < 3)
                {
                    response.Message = "Please enter at least 3 characters";
                    return response;
                }

                if (trimmedQuery.Length > 50) // Reasonable upper limit
                {
                    response.Message = "Search query is too long";
                    return response;
                }

                _logger.LogDebug("Searching CVR for query: {Query}", trimmedQuery);

                var encodedQuery = Uri.EscapeDataString(trimmedQuery);
                var requestUrl = $"api/Cvr/search?query={encodedQuery}";

                using var httpResponse = await _httpClient.GetAsync(requestUrl, cancellationToken);

                if (!httpResponse.IsSuccessStatusCode)
                {
                    response.Message = httpResponse.StatusCode switch
                    {
                        HttpStatusCode.BadRequest => "Invalid search query format",
                        HttpStatusCode.NotFound => "No companies found matching your search",
                        HttpStatusCode.TooManyRequests => "Too many requests. Please wait a moment before searching again",
                        HttpStatusCode.Unauthorized => "Access denied. Please refresh the page and try again",
                        HttpStatusCode.InternalServerError => "Server error. Please try again later",
                        HttpStatusCode.ServiceUnavailable => "CVR service is temporarily unavailable",
                        HttpStatusCode.RequestTimeout => "Request timed out. Please try again",
                        _ => "Unable to search CVR database. Please try again later"
                    };

                    _logger.LogWarning("CVR search failed with status {Status} for query: {Query}",
                        httpResponse.StatusCode, trimmedQuery);
                    return response;
                }

                var jsonResponse = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

                if (string.IsNullOrWhiteSpace(jsonResponse))
                {
                    response.Message = "Received empty response from server";
                    _logger.LogWarning("Empty response received for CVR query: {Query}", trimmedQuery);
                    return response;
                }

                // Try to deserialize as the new CvrSearchResponse first
                CvrSearchResponse? serverResponse = null;
                List<CvrAutoCompleteItem>? results = null;

                try
                {
                    // First, try the new response format
                    serverResponse = JsonSerializer.Deserialize<CvrSearchResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                    });

                    if (serverResponse != null)
                    {
                        results = serverResponse.Results;
                        if (!serverResponse.IsSuccess)
                        {
                            response.Message = serverResponse.Message ?? "Search failed";
                            _logger.LogWarning("Server returned error for CVR query {Query}: {Message}",
                                trimmedQuery, serverResponse.Message);
                            return response;
                        }
                    }
                }
                catch (JsonException)
                {
                    // Fall back to legacy list format
                    try
                    {
                        results = JsonSerializer.Deserialize<List<CvrAutoCompleteItem>>(jsonResponse, new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true,
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
                        });
                    }
                    catch (JsonException ex)
                    {
                        response.Message = "Received invalid data format from server";
                        _logger.LogError(ex, "Failed to deserialize CVR response in any format for query: {Query}. Response: {Response}",
                            trimmedQuery, jsonResponse);
                        return response;
                    }
                }

                if (results == null)
                {
                    response.Message = "Failed to parse server response";
                    _logger.LogError("Null results after deserialization for query: {Query}", trimmedQuery);
                    return response;
                }

                response.Results = results;
                response.IsSuccess = true;

                // Use server message if available, otherwise generate client message
                response.Message = serverResponse?.Message ?? results.Count switch
                {
                    0 => "No companies found matching your search",
                    1 => "Found 1 company",
                    _ => $"Found {results.Count} companies"
                };

                _logger.LogDebug("CVR search completed successfully. Found {Count} results for query: {Query}",
                    results.Count, trimmedQuery);

                return response;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                response.Message = "Search was cancelled";
                _logger.LogDebug("CVR search was cancelled for query: {Query}", query);
                return response;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                response.Message = "Search timed out. Please try again";
                _logger.LogWarning("CVR search timed out for query: {Query}", query);
                return response;
            }
            catch (HttpRequestException ex)
            {
                response.Message = "Unable to connect to server. Please check your internet connection";
                _logger.LogError(ex, "HTTP error during CVR search for query: {Query}", query);
                return response;
            }
            catch (JsonException ex)
            {
                response.Message = "Received invalid data from server. Please try again";
                _logger.LogError(ex, "JSON parsing error for CVR search response, query: {Query}", query);
                return response;
            }
            catch (Exception ex)
            {
                response.Message = "An unexpected error occurred. Please try again";
                _logger.LogError(ex, "Unexpected error during CVR search for query: {Query}", query);
                return response;
            }
        }

        // Overload for backward compatibility - returns just the list
        public async Task<List<CvrAutoCompleteItem>> SearchCvrLegacyAsync(string query, CancellationToken cancellationToken = default)
        {
            var response = await SearchCvrAsync(query, cancellationToken);
            return response.Results;
        }
    }

    // Server response wrapper class (should match your backend CvrSearchResponse)
    public class CvrSearchResponse
    {
        public List<CvrAutoCompleteItem> Results { get; set; } = new();
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int ResultCount => Results.Count;
    }
    public class CvrClientResponse
    {
        public List<CvrAutoCompleteItem> Results { get; set; } = new();
        public bool IsSuccess { get; set; } = false;
        public string Message { get; set; } = string.Empty;
        public int ResultCount => Results.Count;
        public bool HasResults => Results.Count > 0;
    }

    // Extension methods for easier usage
    public static class CvrClientResponseExtensions
    {
        public static bool IsEmpty(this CvrClientResponse response) => !response.HasResults;

        public static bool IsError(this CvrClientResponse response) => !response.IsSuccess;

        public static CvrAutoCompleteItem? FirstResult(this CvrClientResponse response) =>
            response.HasResults ? response.Results.First() : null;
    }
}