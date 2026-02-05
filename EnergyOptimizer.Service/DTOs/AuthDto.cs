using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.API.DTOs
{
    public class AuthDto
    {
        public record RegisterDto(
            [Required(ErrorMessage = "Full name is required")]
            [MinLength(3, ErrorMessage = "Full name must be at least 3 characters")]
            string FullName,

            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            string Email,

            [Required(ErrorMessage = "Password is required")]
            [MinLength(6, ErrorMessage = "Password must be at least 6 characters")]
            string Password
        );

        public record LoginDto(
            [Required(ErrorMessage = "Email is required")]
            [EmailAddress(ErrorMessage = "Invalid email format")]
            string Email,

            [Required(ErrorMessage = "Password is required")]
            string Password
        );

        public record AuthResponseDto(
            bool IsSuccess,
            string Message,
            string? Token = null
        );
    }
}