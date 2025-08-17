using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlazorWebPoc.ApiService.Model
{
    [Table("UserTokens")]
    public class UserToken
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public UserAccounts User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public DateTime Expiration { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
