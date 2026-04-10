using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class AuthnResponseDto
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public List<string> Roles { get; set; } = new List<string>();

        public bool IsVerified { get; set; }

        public string JWToken { get; set; } = string.Empty;
    }
}
