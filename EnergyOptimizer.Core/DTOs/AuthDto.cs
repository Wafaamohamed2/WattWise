namespace EnergyOptimizer.Core.DTOs
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

        public record ForgotPasswordDto(
            string Email
        );

        public record ResetPasswordDto(
            string Email,
            string Token,
            string NewPassword
        );

        public record ChangePasswordDto(
            string CurrentPassword,
            string NewPassword
        );

        public record AuthResponseDto(
            bool IsSuccess,
            string Message,
            string? Token = null
        );
    }
}