using AutoMapper;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Exceptions;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.Auth.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;


namespace EnergyOptimizer.Core.Features.Auth.Handlers
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;

        public RegisterCommandHandler(UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _userManager = userManager;
            _mapper = mapper;
        }

        public async Task<ApiResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var user = _mapper.Map<ApplicationUser>(request.Dto);

            var result = await _userManager.CreateAsync(user, request.Dto.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            return new ApiResponse(200, "User registered successfully!");
        }
    }
}
