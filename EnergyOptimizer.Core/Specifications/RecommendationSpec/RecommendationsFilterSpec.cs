using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Core.Specifications.RecommendationSpec
{
    public class RecommendationsFilterSpec : BaseSpecifcation<EnergyRecommendation>
    {
        public RecommendationsFilterSpec(bool? isImplemented)
            : base(r =>
                !isImplemented.HasValue || r.IsImplemented == isImplemented.Value)
        {
            ApplyOrderByDescending(r => r.Priority);
        }
    }
}
