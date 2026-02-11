using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Interfaces;
using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;


namespace EnergyOptimizer.Core.Features.AI.Handlers.RecommendationHelpers
{
    public class DeleteRecommendationHandler : IRequestHandler<DeleteRecommendationCommand, ApiResponse>
    {
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;

        public DeleteRecommendationHandler(IGenericRepository<EnergyRecommendation> recommendationRepo)
        {
            _recommendationRepo = recommendationRepo;
        }

        public async Task<ApiResponse> Handle(DeleteRecommendationCommand request, CancellationToken ct)
        {
            var rec = await _recommendationRepo.GetByIdAsync(request.Id);
            if (rec == null) return new ApiResponse(404, "Recommendation not found");

            _recommendationRepo.Delete(rec);
            await _recommendationRepo.SaveChangesAsync();
            return new ApiResponse(200, "Deleted successfully");
        }
    }
}
