using System.Text;
using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.DTOs.ReadingsDTOs;
using EnergyOptimizer.Core.Features.AI.Queries.ReadingsQueries;
using EnergyOptimizer.Core.Specifications.ReadSpec;
using EnergyOptimizer.Core.Features.AI.Commands;

namespace EnergyOptimizer.Core.Features.AI.Handlers.ReadingsHandlers
{
    public class ExportReadingsHandler : IRequestHandler<ExportReadingsQuery, ApiResponse>
    {
        private readonly IGenericRepository<EnergyReading> _readingRepo;
        private readonly ICurrentUserService _currentUser;

        public ExportReadingsHandler(IGenericRepository<EnergyReading> readingRepo, ICurrentUserService currentUser)
        {
            _readingRepo = readingRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(ExportReadingsQuery request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();

            DateTime start = string.IsNullOrEmpty(request.StartDate)
                ? DateTime.UtcNow.AddDays(-30).Date
                : DateTime.Parse(request.StartDate);
            DateTime end = string.IsNullOrEmpty(request.EndDate)
                ? DateTime.UtcNow
                : DateTime.Parse(request.EndDate);

            if ((end - start).TotalDays > 30)
                throw new BadRequestException("Export range cannot exceed 30 days.");

            var readings = request.DeviceId.HasValue
                ? await _readingRepo.ListAsync(new ReadingsByDeviceAndDateSpec(request.DeviceId.Value, start, end, userId))
                : await _readingRepo.ListAsync(new ReadingsByDateRangeSpec(start, end, userId));

            var csv = new StringBuilder();
            csv.AppendLine("DeviceId,DeviceName,PowerConsumptionKW,Voltage,Current,Temperature,Timestamp");
            foreach (var r in readings)
            {
                csv.AppendLine($"{r.DeviceId},{r.Device?.Name},{r.PowerConsumptionKW},{r.Voltage},{r.Current},{r.Temperature},{r.Timestamp:O}");
            }

            var bytes = Encoding.UTF8.GetBytes(csv.ToString());
            var fileName = $"readings_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

            return new ApiResponse(200, "Export generated successfully",
                new ExportResultDto(bytes, fileName));
        }
    }
}
