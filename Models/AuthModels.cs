using System.ComponentModel.DataAnnotations;

namespace ProMeet.Models
{
    // Request Models
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }

        [Required]
        public string UserType { get; set; } = "Client"; // "Client" or "Professional"
    }

    public class GoogleAuthRequest
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
        
        public string? UserType { get; set; } = "Client";
    }

    // Response Models
    public class AuthResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? Token { get; set; }
        public DateTime? Expiration { get; set; }
        public UserInfo? User { get; set; }
    }

    public class UserInfo
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public string? PhotoURL { get; set; }
    }
}
