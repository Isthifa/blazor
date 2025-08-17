using System.ComponentModel.DataAnnotations;

namespace BlazorWebPoc.ApiService.Model.Request
{
    public class RegisterAccount
    {

        public string? UserName { get; set; }  // Public so EF Core maps it


        public string? Password { get; set; }


        public string? Email { get; set; }  // Matches the Index above

        public string? PhoneNumber { get; set; }

        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword
        {
            get; set;
        }
        public RoleType Role { get; set; } = RoleType.User; // Default role is User
    }
 }
