using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class ResetPasswordRequestDto
    {
        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required]
        public string NewPassword { get; set; } = string.Empty;

        [Required]
        [Compare("NewPassword", ErrorMessage="The Password and confirmed Password do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
