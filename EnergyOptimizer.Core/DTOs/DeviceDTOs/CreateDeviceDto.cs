using EnergyOptimizer.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.Core.DTOs.DeviceDTOs
{
    public class CreateDeviceDto
    {
        [Required(ErrorMessage = "Device name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Zone ID is required")]
        public int ZoneId { get; set; }
        public DeviceType Type { get; set; }

        [Required(ErrorMessage = "Rated Power is required")]
        [Range(0.01, 50.0, ErrorMessage = "Rated Power must be between 0.01 and 50.0 KW")]
        public decimal RatedPowerKW { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? InstallationDate { get; set; }
    }
}
