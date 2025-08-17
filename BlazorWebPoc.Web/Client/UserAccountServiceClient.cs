using BlazorWebPoc.ApiService.Model.Reponse;
using BlazorWebPoc.ApiService.Model.Request;

namespace BlazorWebPoc.Web.Client
{
    public class UserAccountServiceClient 
    {
        private readonly HttpClient _httpClient;
        public UserAccountServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var response = await _httpClient.PostAsJsonAsync("api/user/login", loginRequest);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }
            else
            {
                var errorMessage = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"Login failed: {errorMessage}");
            }
        }

        public Task<LoginResponse> RefreshToken(string token)
        {
            throw new NotImplementedException("RefreshToken method is not implemented yet.");
        }

        public Task<RegisterAccount> RegisterAccount(RegisterAccount registerAccount)
        {
            var response = _httpClient.PostAsJsonAsync("api/user/register", registerAccount);
            if (response.Result.IsSuccessStatusCode)
            {
                return response.Result.Content.ReadFromJsonAsync<RegisterAccount>();
            }
            else
            {
                var errorMessage = response.Result.Content.ReadAsStringAsync().Result;
                throw new HttpRequestException($"Registration failed: {errorMessage}");
            }
        }
    }
}
