using BlazorWebPoc.ApiService.Data;
using BlazorWebPoc.ApiService.Model;
using BlazorWebPoc.ApiService.Model.Reponse;
using BlazorWebPoc.ApiService.Model.Request;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BlazorWebPoc.ApiService.Service.impl
{
    public class UserAccountServiceImpls : IUserAccountService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public UserAccountServiceImpls(AppDbContext appDbContext,IConfiguration configuration)
        {
            _appDbContext = appDbContext;
            _configuration = configuration;
        }

        public async Task<RegisterAccount> RegisterAccount(RegisterAccount registerAccount)
        {
            // Map request model to entity
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerAccount.Password);
            var entity = new UserAccounts
            {
                UserName = registerAccount.UserName,
                Password = hashedPassword,
                Email = registerAccount.Email,
                PhoneNumber = registerAccount.PhoneNumber
            };
            RoleType role = registerAccount.Role;// Default role is User

            _appDbContext.UserAccounts.Add(entity);

            var userRole = new UserRole
            {
                UserId = entity.Id,
                User = entity,
                Role = role
            };

            try
            {
                _appDbContext.UserRoles.Add(userRole);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error while saving UserRole for UserId {userRole.UserId}", ex);
            }



            await _appDbContext.SaveChangesAsync();

            return registerAccount;
        }

        public async Task<LoginResponse> Login(LoginRequest loginRequest)
        {
            var user = await _appDbContext.UserAccounts
              .FirstOrDefaultAsync(u => u.Email == loginRequest.Email);

            var userRole = await _appDbContext.UserRoles
                .FirstOrDefaultAsync( o=> o.UserId == user.Id);

            var existingToken = await _appDbContext.UserTokens
            .FirstOrDefaultAsync(t => t.UserId == user.Id && t.Expiration > DateTime.UtcNow);
            if(existingToken != null)
            {
                return new LoginResponse
                {
                    IsSuccess = true,
                    Message = "Already logged in",
                    Token = existingToken.Token,
                    Expiration = existingToken.Expiration,
                    UserId = user.Id
                };
            }


            if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.Password))
            {
                return new LoginResponse
                {
                    IsSuccess = false,
                    Message = "Invalid email or password"
                };
            }
            var userRoles = await _appDbContext.UserRoles.Where(o => o.UserId == user.Id).ToListAsync();
            var claims = new List<Claim>
            {
              new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
              new Claim(ClaimTypes.Name, user.UserName),
              new Claim(ClaimTypes.Email, user.Email)
             };

            claims.AddRange(userRoles.Select(r => new Claim(ClaimTypes.Role, r.Role.ToString())));

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
            var exp = DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpirationInMinutes"]));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = exp,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            var userToken = new UserToken
            {
                UserId = user.Id,
                Token = tokenString,
                Expiration = exp
            };

            _appDbContext.UserTokens.Add(userToken);
            await _appDbContext.SaveChangesAsync();

           

            return new LoginResponse
            {
                IsSuccess = true,
                Message = "Login successful",
                Token = tokenString,
                Expiration = exp,
                UserId = user.Id
            };
        
        }

        public async Task<LoginResponse> RefreshToken(string token)
        {
            // You would validate and issue a new token here
            return await Task.FromResult(new LoginResponse
            {
                IsSuccess = true,
                Message = "Token refreshed",
                Token = "new-fake-jwt-token"
            });
        }
    }
}
