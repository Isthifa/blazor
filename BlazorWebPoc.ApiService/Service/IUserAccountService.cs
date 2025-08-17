using BlazorWebPoc.ApiService.Model.Reponse;
using BlazorWebPoc.ApiService.Model.Request;

namespace BlazorWebPoc.ApiService.Service
{
    public interface IUserAccountService
    {
        Task<RegisterAccount> RegisterAccount(RegisterAccount registerAccount);
        Task<LoginResponse> Login(LoginRequest loginRequest);
        Task<LoginResponse> RefreshToken(string token);

    }
}
