using EnergyOptimizer.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace EnergyOptimizer.Core.DTOs.DeviceDTOs
{
    public class UpdateDeviceDto
    {
        [Required(ErrorMessage = "Device name is required")]
        [StringLength(100, ErrorMessage = "Name is too long")]
        public string Name { get; set; }
        public int? ZoneId { get; set; }
        public DeviceType? Type { get; set; }

        [Required(ErrorMessage = "Rated Power is required")]
        [Range(0.01, 100.0, ErrorMessage = "Rated Power must be between 0.01 and 100.0 KW")]
        public decimal? RatedPowerKW { get; set; }
        public bool? IsActive { get; set; }
    }
}

