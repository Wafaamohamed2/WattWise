using EnergyOptimizer.Core.Entities.AI_Analysis;

namespace EnergyOptimizer.Core.Specifications.RecommendationSpec
{
    public class RecommendationsCountSpec : BaseSpecifcation<EnergyRecommendation>
    {
        public RecommendationsCountSpec(bool? isImplemented)
            : base(r => !isImplemented.HasValue || r.IsImplemented == isImplemented.Value)
        {
        }
    }
}
