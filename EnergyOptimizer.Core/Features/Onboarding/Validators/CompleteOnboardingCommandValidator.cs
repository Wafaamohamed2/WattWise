using FluentValidation;
using EnergyOptimizer.Core.Features.Onboarding.Commands;

namespace EnergyOptimizer.Core.Features.Onboarding.Validators
{
    public class CompleteOnboardingCommandValidator : AbstractValidator<CompleteOnboardingCommand>
    {
        public CompleteOnboardingCommandValidator()
        {
            RuleFor(x => x.Dto.BuildingId)
                .GreaterThan(0).WithMessage("Valid building ID is required.");

            RuleFor(x => x.Dto.Devices)
                .NotEmpty().WithMessage("At least one device must be selected during onboarding.");

            RuleForEach(x => x.Dto.Devices).ChildRules(device =>
            {
                device.RuleFor(d => d.Name)
                    .NotEmpty().WithMessage("Device name is required.")
                    .MaximumLength(100).WithMessage("Device name cannot exceed 100 characters.");

                device.RuleFor(d => d.Quantity)
                    .GreaterThan(0).WithMessage("Device quantity must be at least 1.");

                device.RuleFor(d => d.RatedPowerKW)
                    .GreaterThan(0).WithMessage("Rated power (kW) must be greater than 0.");

                device.RuleFor(d => d.DeviceType)
                    .IsInEnum().WithMessage("A valid device type must be specified.");

                device.RuleFor(d => d.ZoneType)
                    .IsInEnum().WithMessage("A valid zone type must be specified.");
            });
        }
    }
}
