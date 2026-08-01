using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Commands.BuildingCommands
{
    public class CreateBuildingCommandValidator : AbstractValidator<CreateBuildingCommand>
    {
        public CreateBuildingCommandValidator()
        {
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
