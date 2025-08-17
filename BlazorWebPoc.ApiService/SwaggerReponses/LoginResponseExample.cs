using BlazorWebPoc.ApiService.Model.Reponse;
using Swashbuckle.AspNetCore.Filters;

namespace BlazorWebPoc.ApiService.SwaggerReponses
{
    public class LoginResponseExample : IExamplesProvider<LoginResponse>
    {
        public LoginResponse GetExamples()
        {
            return new LoginResponse
            {
                Token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
                Expiration = DateTime.UtcNow.AddMinutes(60),
                UserId = Guid.NewGuid(),
                IsSuccess = true,
                Message = "Login successful"
            };
        }
    }

}
