using EnergyOptimizer.AI.Services;
using EnergyOptimizer.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EnergyOptimizer.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {

        private readonly PatternDetectionService _patternService;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<AIController> _logger;

        public AIController(
            PatternDetectionService patternService,
            IGeminiService geminiService,
            ILogger<AIController> logger)
        {
            _patternService = patternService;
            _geminiService = geminiService;
            _logger = logger;
        }

        [HttpPost("analyze-patterns")]
        public async Task<ActionResult<object>> AnalyzePatterns(
          [FromQuery] DateTime? startDate = null,
          [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-7);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest(new { error = "Start date must be before end date" });
                }

                if ((end - start).TotalDays > 90)
                {
                    return BadRequest(new { error = "Maximum analysis period is 90 days" });
                }
                _logger.LogInformation("Analyzing patterns from {Start} to {End}", start, end);

                var result = await _patternService.AnalyzeConsumptionPatterns(start, end);

                if (!result.Success)
                {
                    return BadRequest(new
                    {
                        success = false,
                        error = result.ErrorMessage
                    });
                }

                return Ok(new
                {
                    success = true,
                    period = new
                    {
                        startDate = start.ToString("yyyy-MM-dd"),
                        endDate = end.ToString("yyyy-MM-dd")
                    },
                    analysis = new
                    {
                        summary = result.Summary,
                        insights = result.Insights,
                        recommendations = result.Recommendations,
                        metrics = result.Metrics
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing patterns");
                return StatusCode(500, new { error = "Failed to analyze patterns" });
            }
        }

        [HttpPost("detect-anomalies/{deviceId}")]
        public async Task<ActionResult<object>> DetectAnomalies(
            int deviceId,
            [FromQuery] int days = 7)
        {
            try
            {
                if (days < 1 || days > 30)
                {
                    return BadRequest(new { error = "Days must be between 1 and 30" });
                }

                _logger.LogInformation("Detecting anomalies for device {DeviceId}", deviceId);

                var result = await _patternService.DetectDeviceAnomalies(deviceId, days);

                return Ok(new
                {
                    deviceId,
                    daysAnalyzed = days,
                    hasAnomalies = result.HasAnomalies,
                    anomaliesCount = result.Anomalies.Count,
                    anomalies = result.Anomalies.Select(a => new
                    {
                        timestamp = a.Timestamp,
                        actualValue = a.ActualValue,
                        expectedValue = a.ExpectedValue,
                        deviation = a.Deviation,
                        severity = a.Severity,
                        description = a.Description
                    }),
                    analysis = result.Analysis
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalies for device {DeviceId}", deviceId);
                return StatusCode(500, new { error = "Failed to detect anomalies" });
            }
        }

        [HttpPost("generate-recommendations")]
        public async Task<ActionResult<object>> GenerateRecommendations(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            try
            {
                var start = startDate ?? DateTime.UtcNow.AddDays(-30);
                var end = endDate ?? DateTime.UtcNow;

                if (start >= end)
                {
                    return BadRequest(new { error = "Start date must be before end date" });
                }

                _logger.LogInformation("Generating recommendations from {Start} to {End}", start, end);

                var result = await _patternService.GenerateRecommendations(start, end);

                return Ok(new
                {
                    success = true,
                    period = new
                    {
                        startDate = start.ToString("yyyy-MM-dd"),
                        endDate = end.ToString("yyyy-MM-dd")
                    },
                    recommendations = result.Recommendations.Select(r => new
                    {
                        title = r.Title,
                        description = r.Description,
                        priority = r.Priority,
                        potentialSavings = r.PotentialSavingsKWh,
                        actionItems = r.ActionItems
                    }),
                    estimatedSavings = new
                    {
                        kWh = result.EstimatedSavingsKWh,
                        percent = result.EstimatedSavingsPercent
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
                return StatusCode(500, new { error = "Failed to generate recommendations" });
            }
        }

        [HttpPost("predict-consumption")]
        public async Task<ActionResult<object>> PredictConsumption(
           [FromQuery] int days = 7)
        {
            try
            {
                if (days < 1 || days > 30)
                {
                    return BadRequest(new { error = "Days must be between 1 and 30" });
                }

                _logger.LogInformation("Predicting consumption for next {Days} days", days);

                var result = await _patternService.PredictConsumption(days);


                return Ok(new
                {
                    predictionDate = result.PredictionDate.ToString("yyyy-MM-dd"),
                    predictedConsumptionKWh = result.PredictedConsumptionKWh,
                    confidenceScore = result.ConfidenceScore,
                    explanation = result.Explanation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting consumption");
                return StatusCode(500, new { error = "Failed to predict consumption" });
            }
        }

        [HttpPost("ask")]
        public async Task<ActionResult<object>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Question))
                {
                    return BadRequest(new { error = "Question is required" });
                }

                _logger.LogInformation("Processing question: {Question}", request.Question);

                var answer = await _geminiService.AskQuestion(
                    request.Question,
                    request.Context ?? "Energy optimization system");

                return Ok(new
                {
                    question = request.Question,
                    answer
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question");
                return StatusCode(500, new { error = "Failed to process question" });
            }
        }

        [HttpGet("analysis-history")]
        public async Task<ActionResult<object>> GetAnalysisHistory(
           [FromQuery] int page = 1,
           [FromQuery] int pageSize = 10)
        {
            try
            {
                return Ok(new
                {
                    page,
                    pageSize,
                    totalCount = 0,
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving analysis history");
                return StatusCode(500, new { error = "Failed to retrieve history" });
            }
        }

        [HttpGet("recommendations")]
        public async Task<ActionResult<object>> GetRecommendations(
        [FromQuery] bool? isImplemented = null,
        [FromQuery] string? priority = null)
        {
            try
            {
                return Ok(new
                {
                    totalCount = 0,
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recommendations");
                return StatusCode(500, new { error = "Failed to retrieve recommendations" });
            }
        }

        [HttpPatch("recommendations/{id}/implement")]
        public async Task<ActionResult<object>> ImplementRecommendation(int id)
        {
            try
            {
                return Ok(new
                {
                    id,
                    message = "Recommendation marked as implemented"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error implementing recommendation");
                return StatusCode(500, new { error = "Failed to implement recommendation" });
            }
        }

        [HttpGet("anomalies")]
        public async Task<ActionResult<object>> GetAnomalies(
            [FromQuery] bool? isResolved = null,
            [FromQuery] string? severity = null,
            [FromQuery] int? deviceId = null)
        {
            try
            {
                return Ok(new
                {
                    totalCount = 0,
                    data = new List<object>()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving anomalies");
                return StatusCode(500, new { error = "Failed to retrieve anomalies" });
            }
        }

        [HttpPatch("anomalies/{id}/resolve")]
        public async Task<ActionResult<object>> ResolveAnomaly(
           int id,
           [FromBody] ResolveAnomalyRequest request)
        {
            try
            {
                return Ok(new
                {
                    id,
                    message = "Anomaly marked as resolved"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resolving anomaly");
                return StatusCode(500, new { error = "Failed to resolve anomaly" });
            }
        }

        public record class AskQuestionRequest(
                  string Question,
                  string? Context = null
          );

        public record class ResolveAnomalyRequest(
            string ResolutionNotes
        );
    
    }
}
