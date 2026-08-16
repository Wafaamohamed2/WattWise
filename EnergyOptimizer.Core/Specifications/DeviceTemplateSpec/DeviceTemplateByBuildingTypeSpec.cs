using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;

namespace EnergyOptimizer.Core.Specifications.DeviceTemplateSpec
{
    public class DeviceTemplateByBuildingTypeSpec : BaseSpecifcation<DeviceTemplate>
    {
        public DeviceTemplateByBuildingTypeSpec(BuildingType buildingType)
            : base(t => t.BuildingType == buildingType)
        {
        }
    }
}
