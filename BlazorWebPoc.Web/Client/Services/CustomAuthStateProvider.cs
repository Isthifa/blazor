using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace BlazorWebPoc.Web.Client.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider
    {
        private ClaimsPrincipal _anonymous = new ClaimsPrincipal(new ClaimsIdentity());
        private string? _token;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (string.IsNullOrWhiteSpace(_token))
            {
                return Task.FromResult(new AuthenticationState(_anonymous));
            }

            var handler = new JwtSecurityTokenHandler();
            JwtSecurityToken jwt;

            try
            {
                jwt = handler.ReadJwtToken(_token);
            }
            catch
            {
                return Task.FromResult(new AuthenticationState(_anonymous));
            }

            // Check expiry
            if (jwt.ValidTo < DateTime.UtcNow)
            {
                _token = null;
                return Task.FromResult(new AuthenticationState(_anonymous));
            }

            var identity = new ClaimsIdentity(jwt.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return Task.FromResult(new AuthenticationState(user));
        }

        public void SetToken(string token)
        {
            _token = token;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }

        public void ClearToken()
        {
            _token = null;
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
        }

        public string? GetToken()
        {
            return _token;
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrWhiteSpace(_token);
        }
    }
}
