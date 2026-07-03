using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public class UpdateDeviceCommandValidator : AbstractValidator<UpdateDeviceCommand>
    {
        public UpdateDeviceCommandValidator()
        {
            RuleFor(x => x.id)
                .GreaterThan(0).WithMessage("Device ID must be greater than 0.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Device name is required.")
                .MaximumLength(100).WithMessage("Name is too long.");

            RuleFor(x => x.Dto.RatedPowerKW)
                .NotNull().WithMessage("Rated Power is required.")
                .InclusiveBetween(0.01m, 100.0m).WithMessage("Rated Power must be between 0.01 and 100.0 KW.");

            RuleFor(x => x.Dto.ZoneId)
                .GreaterThan(0).WithMessage("A valid Zone ID is required.")
                .When(x => x.Dto.ZoneId.HasValue);
        }
    }
}
