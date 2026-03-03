using MediatR;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions; 
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers
{
    public class ImplementRecommendationHandler : IRequestHandler<ImplementRecommendationCommand, ApiResponse>
    {
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;

        public ImplementRecommendationHandler(IGenericRepository<EnergyRecommendation> recommendationRepo)
        {
            _recommendationRepo = recommendationRepo;
        }

        public async Task<ApiResponse> Handle(ImplementRecommendationCommand request, CancellationToken ct)
        {
            var rec = await _recommendationRepo.GetByIdAsync(request.Id);

            if (rec == null)
                throw new NotFoundException($"Recommendation with ID {request.Id} not found");

            rec.IsImplemented = true;
            rec.ImplementedDate = DateTime.UtcNow;

            _recommendationRepo.Update(rec);
            await _recommendationRepo.SaveChangesAsync();

            return new ApiResponse(200, "Marked as implemented");
        }
    }
}