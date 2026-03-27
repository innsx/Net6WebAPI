using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AuthenticationRequest
    {
        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
