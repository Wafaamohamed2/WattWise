using FluentValidation;

namespace EnergyOptimizer.Core.Features.Zones.Commands
{
    public class CreateZoneCommandValidator : AbstractValidator<CreateZoneCommand>
    {
        public CreateZoneCommandValidator()
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Zone name is required.")
                .MaximumLength(200).WithMessage("Zone name cannot exceed 200 characters.");

            RuleFor(x => x.Dto.BuildingId)
                .GreaterThan(0).WithMessage("Valid building ID is required.");

            RuleFor(x => x.Dto.Area)
                .GreaterThan(0).WithMessage("Zone area must be greater than 0.");

            RuleFor(x => x.Dto.Type)
                .IsInEnum().WithMessage("Valid zone type is required.");
        }
    }
}
