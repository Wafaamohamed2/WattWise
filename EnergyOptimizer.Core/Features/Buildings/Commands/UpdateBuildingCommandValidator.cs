using FluentValidation;

namespace EnergyOptimizer.Core.Features.Buildings.Commands
{
    public class UpdateBuildingCommandValidator : AbstractValidator<UpdateBuildingCommand>
    {
        public UpdateBuildingCommandValidator()
        {
            RuleFor(x => x.Id)
                .GreaterThan(0).WithMessage("Valid building ID is required.");

            RuleFor(x => x.Dto.Name)
                .NotEmpty().WithMessage("Building name is required.")
                .MaximumLength(200).WithMessage("Building name cannot exceed 200 characters.");

            RuleFor(x => x.Dto.TotalArea)
                .GreaterThan(0).WithMessage("Total area must be greater than 0.");

            RuleFor(x => x.Dto.NumberOfRooms)
                .GreaterThan(0).WithMessage("Number of rooms must be greater than 0.");
        }
    }
}
