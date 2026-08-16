using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Features.Onboarding.Commands;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Onboarding.Handlers
{
    public class CreateBuildingWithTypeHandler : IRequestHandler<CreateBuildingWithTypeCommand, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly ICurrentUserService _currentUser;

        public CreateBuildingWithTypeHandler(IGenericRepository<Building> buildingRepo, ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CreateBuildingWithTypeCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            var building = new Building
            {
                Name = request.Dto.Name,
                Address = request.Dto.Address,
                Type = request.Dto.Type,
                UserId = userId,
                IsOnboardingComplete = false
            };

            _buildingRepo.Add(building);
            await _buildingRepo.SaveChangesAsync();

            return new ApiResponse(200, "Building created for onboarding", building);
        }
    }
}
