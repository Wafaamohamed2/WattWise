using AutoMapper;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Queries.AlertsQueries;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Specifications.AlertSpec;
using MediatR;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.AlertHandlers
{
    public class GetAlertByIdHandler : IRequestHandler<GetAlertByIdQuery, ApiResponse>
    {
        private readonly IGenericRepository<Alert> _alertRepo;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetAlertByIdHandler(IGenericRepository<Alert> alertRepo, IMapper mapper, ICurrentUserService currentUser)
        {
            _alertRepo = alertRepo;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(GetAlertByIdQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new AlertOwnedByUserSpec(request.Id, userId);
            var alert = await _alertRepo.GetEntityWithSpec(spec);

            if (alert == null) throw new NotFoundException($"Alert with ID {request.Id} not found");

            var dto = _mapper.Map<AlertDto>(alert);
            return new ApiResponse(200, "Alert retrieved successfully", dto);
        }
    }
}
