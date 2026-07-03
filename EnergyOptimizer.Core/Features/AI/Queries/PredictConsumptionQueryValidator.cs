using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public class PredictConsumptionQueryValidator : AbstractValidator<PredictConsumptionQuery>
    {
        public PredictConsumptionQueryValidator()
        {
            RuleFor(x => x.Days)
                .InclusiveBetween(1, 30).WithMessage("Days must be between 1 and 30.");
        }
    }
}
