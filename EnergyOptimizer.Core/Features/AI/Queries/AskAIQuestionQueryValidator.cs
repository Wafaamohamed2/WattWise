using FluentValidation;

namespace EnergyOptimizer.Core.Features.AI.Queries
{
    public class AskAIQuestionQueryValidator : AbstractValidator<AskAIQuestionQuery>
    {
        public AskAIQuestionQueryValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("Question is required.");
        }
    }
}
