using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.RecommendationCommans;
using EnergyOptimizer.Core.Features.AI.Queries;
using EnergyOptimizer.Core.Features.AI.Queries.AnalysisQueries;
using EnergyOptimizer.Core.Features.AI.Queries.AnomaliesQueries;
using EnergyOptimizer.Core.Features.AI.Queries.Reco;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EnergyOptimizer.Core.Exceptions;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AIController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("run-analysis")]
        public async Task<IActionResult> RunAnalysis()
        {
            var result = await _mediator.Send(new RunGlobalAnalysisCommand());
            return Ok(result);
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> RunCleanup()
        {
            var result = await _mediator.Send(new RunAllCleanupTasksCommand());
            return Ok(result);
        }

        #region Action Endpoints
        [HttpGet("analyze-patterns")]
        public async Task<IActionResult> AnalyzePatterns(
          [FromQuery] DateTime? startDate = null,
          [FromQuery] DateTime? endDate = null)
        {
            var result = await _mediator.Send(new AnalyzePatternsQuery(startDate, endDate));
            return Ok(result);
        }

        [HttpPost("detect-anomalies/{deviceId}")]
        public async Task<IActionResult> DetectAnomalies(
            int deviceId,
            [FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
                throw new BadRequestException("Days must be between 1 and 30");

            var result = await _mediator.Send(new DetectDeviceAnomaliesCommand(deviceId, days));
            return Ok(result);
        }

        [HttpPost("generate-recommendations")]
        public async Task<IActionResult> GenerateRecommendations(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _mediator.Send(new GenerateRecommendationsCommand(startDate, endDate));
            return Ok(result);
        }

        [HttpGet("predict-consumption")]
        public async Task<IActionResult> PredictConsumption(
           [FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
                throw new BadRequestException("Days must be between 1 and 30");

            var result = await _mediator.Send(new PredictConsumptionQuery(days));
            return Ok(result);
        }

        [HttpPost("ask")]
        public async Task<IActionResult> AskQuestion([FromBody] AskQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question))
                throw new BadRequestException("Question is required");

            var answer = await _mediator.Send(new AskAIQuestionQuery(request.Question, request.Context));
            return Ok(answer);
        }
        #endregion

        #region Analysis Endpoints
        [HttpGet("analysis-history")]
        public async Task<IActionResult> GetAnalysisHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? analysisType = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
        {
            DateTime? start =  DateTime.TryParse(startDate, out var s) ? s : null;
            DateTime? end =  DateTime.TryParse(endDate, out var e) ? e : null;
            var result = await _mediator.Send(new GetAnalysisHistoryQuery(page, pageSize, analysisType, start, end));
            return Ok(result);
        }

        [HttpGet("analysis/{id}")]
        public async Task<IActionResult> GetAnalysisById(int id)
        {
            var result = await _mediator.Send(new GetAnalysisByIdQuery(id));
            return Ok(result);
        }

        [HttpGet("Statistics")]
        public async Task<IActionResult> GetAIStatistics()
        {
            var result = await _mediator.Send(new GetAIStatisticsQuery());
            return Ok(result);
        }
        #endregion

        #region Recommendations Endpoints
        [HttpGet("recommendations")]
        public async Task<IActionResult> GetRecommendations(
        [FromQuery] bool? isImplemented = null)
        {
            var result = await _mediator.Send(new GetRecommendationsQuery(isImplemented));
            return Ok(result);
        }

        [HttpPatch("recommendations/{id}/implement")]
        public async Task<IActionResult> ImplementRecommendation(int id)
        {
            var result = await _mediator.Send(new ImplementRecommendationCommand(id));
            return Ok(result);
        }

        [HttpDelete("recommendations/{id}")]
        public async Task<IActionResult> DeleteRecommendation(int id)
        {
            var result = await _mediator.Send(new DeleteRecommendationCommand(id));
            return Ok(result);
        }
        #endregion

        #region Anomalies Endpoints
        [HttpGet("anomalies")]
        public async Task<IActionResult> GetAnomalies(
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? severity = null,
        [FromQuery] int? deviceId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var result = await _mediator.Send(new GetAnomaliesQuery(isResolved, severity, deviceId, page, pageSize));
            return Ok(result);
        }

        [HttpGet("anomalies/{id}")]
        public async Task<IActionResult> GetAnomalyById(int id)
        {
            var result = await _mediator.Send(new GetAnomalyByIdQuery(id));
            return Ok(result);
        }

        [HttpPatch("anomalies/{id}/resolve")]
        public async Task<IActionResult> ResolveAnomaly(
           int id,
           [FromBody] ResolveAnomalyRequest request)
        {
            var result = await _mediator.Send(new ResolveAnomalyCommand(id, request.ResolutionNotes));
            return Ok(result);
        }

        [HttpDelete("anomalies/{id}")]
        public async Task<IActionResult> DeleteAnomaly(int id)
        {
            var result = await _mediator.Send(new DeleteAnomalyCommand(id));
            return Ok(result);
        }
        #endregion
    }

    public record class AskQuestionRequest(
              string Question,
              string? Context = null
    );

    public record class ResolveAnomalyRequest(
        string ResolutionNotes
    );
}