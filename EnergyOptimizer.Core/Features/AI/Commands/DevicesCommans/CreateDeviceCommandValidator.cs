using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Commands.DevicesCommans
{
    public class CreateDeviceCommandValidator : AbstractValidator<CreateDeviceCommand>
    {
        public CreateDeviceCommandValidator()
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Device name is required.")
                .Length(2, 100).WithMessage("Name must be between 2 and 100 characters.");

            RuleFor(x => x.Dto.ZoneId)
                .GreaterThan(0).WithMessage("A valid Zone ID is required.");

            RuleFor(x => x.Dto.RatedPowerKW)
                .InclusiveBetween(0.01m, 50.0m).WithMessage("Rated Power must be between 0.01 and 50.0 KW.");
        }
    }
}
