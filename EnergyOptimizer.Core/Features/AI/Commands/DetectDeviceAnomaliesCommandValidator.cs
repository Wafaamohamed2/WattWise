using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Commands
{
    public class DetectDeviceAnomaliesCommandValidator : AbstractValidator<DetectDeviceAnomaliesCommand>
    {
        public DetectDeviceAnomaliesCommandValidator()
        {
            RuleFor(x => x.DeviceId)
                .GreaterThan(0).WithMessage("A valid Device ID is required.");

            RuleFor(x => x.Days)
                .InclusiveBetween(1, 30).WithMessage("Days must be between 1 and 30.");
        }
    }
}
