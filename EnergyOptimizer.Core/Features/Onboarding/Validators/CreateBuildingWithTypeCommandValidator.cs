using FluentValidation;
using EnergyOptimizer.Core.Features.Onboarding.Commands;

namespace EnergyOptimizer.Core.Features.Onboarding.Validators
{
    public class CreateBuildingWithTypeCommandValidator : AbstractValidator<CreateBuildingWithTypeCommand>
    {
        public CreateBuildingWithTypeCommandValidator()
        {
            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Building name is required.")
                .MaximumLength(200).WithMessage("Building name cannot exceed 200 characters.");

            RuleFor(x => x.Dto.Type)
                .IsInEnum().WithMessage("A valid building type (Home or Company) must be selected.");
        }
    }
}
