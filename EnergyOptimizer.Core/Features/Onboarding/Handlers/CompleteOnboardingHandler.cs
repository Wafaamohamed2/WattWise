using MediatR;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.Onboarding.Commands;
using EnergyOptimizer.Core.Specifications.BuildingSpec;
using EnergyOptimizer.Core.Contracts;

namespace EnergyOptimizer.Core.Features.Onboarding.Handlers
{
    public class CompleteOnboardingHandler : IRequestHandler<CompleteOnboardingCommand, ApiResponse>
    {
        private readonly IGenericRepository<Building> _buildingRepo;
        private readonly IGenericRepository<Zone> _zoneRepo;
        private readonly IGenericRepository<Device> _deviceRepo;
        private readonly ICurrentUserService _currentUser;

        public CompleteOnboardingHandler(
            IGenericRepository<Building> buildingRepo,
            IGenericRepository<Zone> zoneRepo,
            IGenericRepository<Device> deviceRepo,
            ICurrentUserService currentUser)
        {
            _buildingRepo = buildingRepo;
            _zoneRepo = zoneRepo;
            _deviceRepo = deviceRepo;
            _currentUser = currentUser;
        }

        public async Task<ApiResponse> Handle(CompleteOnboardingCommand request, CancellationToken ct)
        {
            var userId = _currentUser.RequireUserId();
            var spec = new BuildingOwnedByUserSpec(request.Dto.BuildingId, userId);
            var building = await _buildingRepo.GetEntityWithSpec(spec);

            if (building == null)
                throw new NotFoundException($"Building with ID {request.Dto.BuildingId} not found or access denied.");

            var createdZones = new Dictionary<string, Zone>(StringComparer.OrdinalIgnoreCase);

            foreach (var item in request.Dto.Devices)
            {
                var zoneName = string.IsNullOrWhiteSpace(item.ZoneName) ? "Main Area" : item.ZoneName.Trim();
                var zoneType = ResolveZoneType(item.ZoneType, zoneName);

                if (!createdZones.TryGetValue(zoneName, out var zone))
                {
                    zone = new Zone
                    {
                        Name = zoneName,
                        BuildingId = building.Id,
                        Type = zoneType
                    };
                    _zoneRepo.Add(zone);
                    createdZones[zoneName] = zone;
                }

                int qty = item.Quantity > 0 ? item.Quantity : 1;
                for (int i = 0; i < qty; i++)
                {
                    var deviceName = qty == 1 ? item.Name : $"{item.Name} #{i + 1}";
                    var device = new Device
                    {
                        Name = deviceName,
                        Type = item.DeviceType,
                        RatedPowerKW = (decimal)item.RatedPowerKW,
                        IsActive = true,
                        Zone = zone
                    };
                    _deviceRepo.Add(device);
                }
            }

            building.IsOnboardingComplete = true;
            _buildingRepo.Update(building);

            // Single atomic transaction through shared DbContext
            await _buildingRepo.SaveChangesAsync();

            return new ApiResponse(200, "Onboarding completed successfully", new
            {
                buildingId = building.Id,
                isOnboardingComplete = true
            });
        }

        private static ZoneType ResolveZoneType(ZoneType specifiedType, string zoneName)
        {
            if (Enum.IsDefined(typeof(ZoneType), specifiedType) && specifiedType != 0)
                return specifiedType;

            var lower = zoneName.ToLowerInvariant();
            if (lower.Contains("bed")) return ZoneType.Bedroom;
            if (lower.Contains("kitchen")) return ZoneType.Kitchen;
            if (lower.Contains("bath") || lower.Contains("restroom")) return ZoneType.Bathroom;
            if (lower.Contains("balcony")) return ZoneType.Balcony;
            if (lower.Contains("garage")) return ZoneType.Garage;

            return ZoneType.LivingRoom;
        }
    }
}
