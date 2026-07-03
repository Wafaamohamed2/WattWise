namespace EnergyOptimizer.API.DTOs
{
    public class AuthDto
    {
        public record RegisterDto(
            string FullName,
            string Email,
            string Password
        );

        public record LoginDto(
            string Email,
            string Password
        );

        public record AuthResponseDto(
            bool IsSuccess,
            string Message,
            string? Token = null
        );
    }
}