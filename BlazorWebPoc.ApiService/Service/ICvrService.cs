using BlazorWebPoc.ApiService.Model;
using static BlazorWebPoc.ApiService.Service.impl.CvrServiceImpl;

namespace BlazorWebPoc.ApiService.Service
{
    public interface ICvrService
    {
        Task<CvrSearchResponse> SearchCvrAsync(string query);
        Task<CvrSearchResult?> GetCvrDetailsAsync(string cvrNumber);
    }
}