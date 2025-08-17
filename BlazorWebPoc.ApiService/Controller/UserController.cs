using BlazorWebPoc.ApiService.Data;
using BlazorWebPoc.ApiService.Model.Reponse;
using BlazorWebPoc.ApiService.Model.Request;
using BlazorWebPoc.ApiService.Service;
using BlazorWebPoc.ApiService.SwaggerReponses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Swashbuckle.AspNetCore.Filters;
using System.Security.Claims;

namespace BlazorWebPoc.ApiService.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {

        private readonly IUserAccountService _userAccountService;
        private readonly ILogger<UserController> _logger;

        private readonly AppDbContext _appDbContext;
        public UserController(IUserAccountService userAccountService, ILogger<UserController> logger, AppDbContext appDbContext)
        {
            _userAccountService = userAccountService;
            _logger = logger;
            _appDbContext = appDbContext;
        }

        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterAccount), StatusCodes.Status200OK)]
        public async Task<IActionResult> Register([FromBody] Model.Request.RegisterAccount registerAccount)
        {
            if (registerAccount == null)
            {
                return BadRequest("Invalid registration data.");
            }
            var result = await _userAccountService.RegisterAccount(registerAccount);
            return Ok(result);
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(LoginResponseExample))]
        public async Task<IActionResult> Login([FromBody] Model.Request.LoginRequest loginRequest)
        {
            if (loginRequest == null)
            {
                _logger.LogError("Login request is null.");
                return BadRequest("Invalid login data.");
            }
            var result = await _userAccountService.Login(loginRequest);

            if (result.IsSuccess)
            {
                
                return Ok(result);
            }
            return Unauthorized(result.Message);
        }


        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var authHeader = Request.Headers["Authorization"].ToString();
            var token = authHeader.Replace("Bearer ", "");

            var userToken = await _appDbContext.UserTokens.FirstOrDefaultAsync(t => t.UserId == userId && t.Token == token);

            if (userToken != null)
            {
                _appDbContext.UserTokens.Remove(userToken);
                await _appDbContext.SaveChangesAsync();
            }

            return Ok(new { Message = "Logged out successfully" });
        }

    }
}
