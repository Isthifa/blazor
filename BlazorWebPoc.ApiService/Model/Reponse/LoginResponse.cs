namespace BlazorWebPoc.ApiService.Model.Reponse
{
    public class LoginResponse
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Token { get; set; } // JWT Token for authentication
        public Guid? UserId { get; set; } // User ID for reference
        public DateTime? Expiration { get; set; } // Token expiration time
    }
}
