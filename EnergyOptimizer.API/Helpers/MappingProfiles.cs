using AutoMapper;
using EnergyOptimizer.Core.DTOs.AlertsDTOs;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Enums;
using static EnergyOptimizer.API.DTOs.AuthDto;

namespace EnergyOptimizer.API.Helpers
{
    public class MappingProfiles : Profile
    {
        public MappingProfiles()
        {
            // Account Mapping
            CreateMap<RegisterDto, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow))
                .ForMember(dest => dest.UpdatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));


            // Alerts Mapping 
            CreateMap<Alert, AlertDto>()
                .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.Device != null ? src.Device.Name : "Unknown"))
                .ForMember(dest => dest.ZoneName, opt => opt.MapFrom(src => (src.Device != null && src.Device.Zone != null) ? src.Device.Zone.Name : "Unknown"))
                .ForMember(dest => dest.AlertType, opt => opt.MapFrom(src => src.Type.ToString()))
               .ForMember(dest => dest.SeverityLabel, opt => opt.MapFrom(src =>
                  Enum.GetName(typeof(AlertSeverity), src.Severity) ?? "Unknown"));
        }

    }
}
