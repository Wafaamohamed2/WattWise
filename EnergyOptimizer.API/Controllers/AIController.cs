using EnergyOptimizer.Core.Features.AI.Commands;
using EnergyOptimizer.Core.Features.AI.Commands.Middleware;
using EnergyOptimizer.Core.Features.AI.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AIController> _logger;
    

        public AIController(
             IMediator mediator,
             ILogger<AIController> logger)
        {
            _mediator = mediator;
            _logger = logger;
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
            var result = await _mediator.Send(new RunAllCleanupTasksCommand(HttpContext.RequestAborted));
            return Ok(result);
        }

        #region Action Endpoints
        [HttpPost("analyze-patterns")]
        public async Task<ActionResult<object>> AnalyzePatterns(
          [FromQuery] DateTime? startDate = null,
          [FromQuery] DateTime? endDate = null)
        {
            var result = await _mediator.Send(new AnalyzePatternsQuery(startDate, endDate));

            if (result.StatusCode == 400) return BadRequest(result);
            return Ok(result);
        }


        // Detect anomalies for a specific device
        [HttpPost("detect-anomalies/{deviceId}")]
        public async Task<ActionResult<object>> DetectAnomalies(
            int deviceId,
            [FromQuery] int days = 7)
        {
            if (days < 1 || days > 30) return BadRequest(new ApiResponse(400, "Days must be between 1 and 30"));
            var result = await _mediator.Send(new DetectDeviceAnomaliesCommand(deviceId, days));

            if (result.StatusCode == 400) return BadRequest(result);
            return Ok(result);

        }


        // Generate energy-saving recommendations
        [HttpPost("generate-recommendations")]
        public async Task<ActionResult<object>> GenerateRecommendations(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var result = await _mediator.Send(new GenerateRecommendationsCommand(startDate, endDate));

            if (result.StatusCode == 400) return BadRequest(result);
            return Ok(result);

        }


        // Predict future consumption
        [HttpPost("predict-consumption")]
        public async Task<ActionResult<object>> PredictConsumption(
           [FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
            {
                return BadRequest(new ApiResponse(400, "Days must be between 1 and 30"));
            }

            var result = await _mediator.Send(new PredictConsumptionQuery(days));

            if (result.StatusCode == 400) return BadRequest(result);
            return Ok(result);
        }


        // Ask a question to the AI system about energy optimization
        [HttpPost("ask")]
        public async Task<ActionResult<object>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(new { error = "Question is required" });

            var answer = await _mediator.Send(new AskAIQuestionQuery(request.Question, request.Context));
            return Ok(answer);
        }
        #endregion

        #region Analysis Endpoints
        // Endpoints for managing analysis history, recommendations, and anomalies
        [HttpGet("analysis-history")]
        public async Task<ActionResult<object>> GetAnalysisHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? analysisType = null,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
        {
            var analyses = await _mediator.Send(new GetAnalysisHistoryQuery(page, pageSize, analysisType, null, null));

            return Ok(analyses);
        }

        // Get detailed analysis by ID
        [HttpGet("analysis/{id}")]
        public async Task<ActionResult<object>> GetAnalysisById(int id)
        {
            var result = await _mediator.Send(new GetAnalysisByIdQuery(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("Statistics")]
        public async Task<ActionResult<object>> GetAIStatistics()
        {
             var statistics = await  _mediator.Send(new GetAIStatisticsQuery());
             return Ok(statistics);
            
        }
        #endregion


        #region Recommendations Endpoints
        [HttpGet("recommendations")]
        public async Task<ActionResult<object>> GetRecommendations(
        [FromQuery] bool? isImplemented = null)
        {

            var result = await _mediator.Send(new GetRecommendationsQuery(isImplemented));
            return Ok(result);

        }


        [HttpPatch("recommendations/{id}/implement")]
        public async Task<ActionResult<object>> ImplementRecommendation(int id)
        {
            var result = await _mediator.Send(new ImplementRecommendationCommand(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("recommndations/{id}")]
        public async Task<ActionResult<object>> DeleteRecommendation(int id)
        {
            var result = await _mediator.Send(new DeleteRecommendationCommand(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }
        #endregion


        #region Anomalies Endpoints
        [HttpGet("anomalies")]
        public async Task<ActionResult<object>> GetAnomalies(
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? severity = null,
        [FromQuery] int? deviceId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
            var anomaliesList = await _mediator.Send(new GetAnomaliesQuery(isResolved, severity, deviceId, page, pageSize));
            return Ok(anomaliesList);
        }

        [HttpGet("anomalies/{id}")]
        public async Task<ActionResult<object>> GetAnomalyById(int id)
        {
            var result = await _mediator.Send(new GetAnomalyByIdQuery(id));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        [HttpPatch("anomalies/{id}/resolve")]
        public async Task<ActionResult<object>> ResolveAnomaly(
           int id,
           [FromBody] ResolveAnomalyRequest request)
        {
            var result = await _mediator.Send(new ResolveAnomalyCommand(id, request.ResolutionNotes));
            if (result.StatusCode == 404) return NotFound(result);
            return Ok(result);
        }

        [HttpDelete("anomalies/{id}")]
        public async Task<ActionResult<object>> DeleteAnomaly(int id)
        {
            var result = await _mediator.Send(new DeleteAnomalyCommand(id));
            if (result.StatusCode == 404) return NotFound(result);
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