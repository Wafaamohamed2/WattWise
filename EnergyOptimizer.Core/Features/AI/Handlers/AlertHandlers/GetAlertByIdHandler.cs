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


        public GetAlertByIdHandler(IGenericRepository<Alert> alertRepo , IMapper mapper)
        {
            _alertRepo = alertRepo;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(GetAlertByIdQuery request, CancellationToken ct)
        {
            var spec = new AlertsWithFiltersSpec(null, null, null, DateTime.MinValue, DateTime.MaxValue);  

            var alert = (await _alertRepo.ListAsync(spec)).FirstOrDefault(a => a.Id == request.Id);

            if (alert == null) throw new NotFoundException($"Alert with ID {request.Id} not found");

            var dto = _mapper.Map<AlertDto>(alert);

            return new ApiResponse(200, "Alert retrieved successfully", dto);
        }
    }
}
