using BlazorWebPoc.ApiService.Model;

namespace BlazorWebPoc.ApiService.Service
{
    public interface ICvrService
    {
        Task<List<CvrAutoCompleteItem>> SearchCvrAsync(string query);
        Task<CvrSearchResult?> GetCvrDetailsAsync(string cvrNumber);
    }
}