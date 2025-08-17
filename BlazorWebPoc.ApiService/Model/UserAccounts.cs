using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorWebPoc.ApiService.Model
{
    [Table("UserAccounts")]
    [Index(nameof(Email), IsUnique = true)] // Unique Email
    public class UserAccounts
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Required]
        public string? UserName { get; set; }  // Public so EF Core maps it

        [Required]
        public string? Password { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }  // Matches the Index above

        public string? PhoneNumber { get; set; }
       

    }

}
