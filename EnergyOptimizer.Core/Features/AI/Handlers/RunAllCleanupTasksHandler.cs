using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Service.Services.Abstract;
using MediatR;

namespace EnergyOptimizer.Core.Features.AI.Handlers
{
    public class RunAllCleanupTasksHandler : IRequestHandler<RunAllCleanupTasksCommand, ApiResponse>
    {
        private readonly IDataCleanupService _cleanupService;

        public RunAllCleanupTasksHandler(IDataCleanupService cleanupService)
        {
            _cleanupService = cleanupService;
        }

        public async Task<ApiResponse> Handle(RunAllCleanupTasksCommand request, CancellationToken ct)
        {
            await _cleanupService.RunAllCleanupTasks(request.CancellationToken);
            return new ApiResponse(200, "Cleanup tasks completed successfully");
        }
    }
}
