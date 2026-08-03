using FluentValidation;

namespace EnergyOptimizer.Core.Features.Zones.Commands
{
    public class UpdateZoneCommandValidator : AbstractValidator<UpdateZoneCommand>
    {
        public UpdateZoneCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid zone ID is required.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Zone name cannot be empty.")
                .MaximumLength(200).WithMessage("Zone name cannot exceed 200 characters.");

            When(x => x.Dto.Area.HasValue, () =>
            {
                RuleFor(x => x.Dto.Area!.Value)
                    .GreaterThan(0).WithMessage("Zone area must be greater than 0.");
            });

            When(x => x.Dto.Type.HasValue, () =>
            {
                RuleFor(x => x.Dto.Type!.Value)
                    .IsInEnum().WithMessage("Valid zone type is required.");
            });
        }
    }
}
