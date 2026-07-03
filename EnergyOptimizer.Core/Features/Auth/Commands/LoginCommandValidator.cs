using FluentValidation;

namespace EnergyOptimizer.Core.Features.Auth.Commands
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {
        public LoginCommandValidator()
        {
            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");

            RuleFor(x => x.Dto.Password)
                .NotEmpty().WithMessage("Password is required.");
        }
    }
}
