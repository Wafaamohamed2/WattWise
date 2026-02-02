using EnergyOptimizer.API.Services;
using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {

        private readonly PatternDetectionService _patternService;
        private readonly IGeminiService _geminiService;
        private readonly ILogger<AIController> _logger;
        private readonly IAIAnalysisService _aiService;
        private readonly IDataCleanupService _cleanupService;
        private readonly IGenericRepository<EnergyAnalysis> _analysisRepo;
        private readonly IGenericRepository<EnergyRecommendation> _recommendationRepo;
        private readonly IGenericRepository<DetectedAnomaly> _anomalyRepo;
        private readonly IGenericRepository<Device> _deviceRepo;

        public AIController(
             PatternDetectionService patternService,
             IGeminiService geminiService,
             IAIAnalysisService aiService,
             IDataCleanupService cleanupService,
             IGenericRepository<EnergyAnalysis> analysisRepo,
             IGenericRepository<EnergyRecommendation> recommendationRepo,
             IGenericRepository<DetectedAnomaly> anomalyRepo,
             IGenericRepository<Device> deviceRepo,
             ILogger<AIController> logger)
        {
            _patternService = patternService;
            _geminiService = geminiService;
            _aiService = aiService;
            _cleanupService = cleanupService;
            _analysisRepo = analysisRepo;
            _recommendationRepo = recommendationRepo;
            _anomalyRepo = anomalyRepo;
            _deviceRepo = deviceRepo;
            _logger = logger;
        }

        [HttpPost("run-analysis")]
        public async Task<IActionResult> RunAnalysis()
        {
            await _aiService.RunGlobalAnalysisAsync(default);
            return Ok(new { message = "AI Analysis started successfully" });
        }

        [HttpPost("cleanup")]
        public async Task<IActionResult> RunCleanup()
        {
            await _cleanupService.RunAllCleanupTasks(default);
            return Ok(new { message = "Data cleanup completed successfully" });
        }
        #region Action Endpoints
        [HttpPost("analyze-patterns")]
        public async Task<ActionResult<object>> AnalyzePatterns(
          [FromQuery] DateTime? startDate = null,
          [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start >= end)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            if ((end - start).TotalDays > 90)
            {
                return BadRequest(new { error = "Maximum analysis period is 90 days" });
            }

            var result = await _patternService.AnalyzeConsumptionPatterns(start, end);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    success = false,
                    error = result.ErrorMessage
                });
            }
            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = "Pattern",
                PeriodStart = start,
                PeriodEnd = end,
                Summary = result.Summary,
                FullResponse = JsonSerializer.Serialize(result)
            };

            await _analysisRepo.AddAsync(analysis);
            await _analysisRepo.SaveChangesAsync();

            return Ok(new { success = true, analysis = result });
        }


        // Detect anomalies for a specific device
        [HttpPost("detect-anomalies/{deviceId}")]
        public async Task<ActionResult<object>> DetectAnomalies(
            int deviceId,
            [FromQuery] int days = 7)
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


        // Generate energy-saving recommendations
        [HttpPost("generate-recommendations")]
        public async Task<ActionResult<object>> GenerateRecommendations(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-30);
            var end = endDate ?? DateTime.UtcNow;

            if (start >= end)
            {
                return BadRequest(new { error = "Start date must be before end date" });
            }

            var result = await _patternService.GenerateRecommendations(start, end);

            // Save to DB
            var analysis = new EnergyAnalysis
            {
                AnalysisDate = DateTime.UtcNow,
                AnalysisType = "Recommendations",
                Summary = $"Generated {result.Recommendations.Count} recommendations",
                FullResponse = JsonSerializer.Serialize(result),
                PeriodStart = start,
                PeriodEnd = end,
                TotalConsumptionKWh = 0,
                DevicesAnalyzed = 0
            };
            await _analysisRepo.AddAsync(analysis);
            await _analysisRepo.SaveChangesAsync();

            foreach (var rec in result.Recommendations)
            {
                await _recommendationRepo.AddAsync(new EnergyRecommendation
                {
                    Title = rec.Title,
                    Description = rec.Description,
                    Category = rec.Category,
                    Priority = rec.Priority,
                    EstimatedSavingsKWh = rec.PotentialSavingsKWh,
                    AnalysisId = analysis.Id
                });
            }

            await _recommendationRepo.SaveChangesAsync();
            return Ok(new { success = true, count = result.Recommendations.Count });
        }


        // Predict future consumption
        [HttpPost("predict-consumption")]
        public async Task<ActionResult<object>> PredictConsumption(
           [FromQuery] int days = 7)
        {
            if (days < 1 || days > 30)
            {
                return BadRequest(new { error = "Days must be between 1 and 30" });
            }

            var result = await _patternService.PredictConsumption(days);

            return Ok(new
            {
                predictionDate = result.PredictionDate.ToString("yyyy-MM-dd"),
                predictedConsumptionKWh = result.PredictedConsumptionKWh,
                confidenceScore = result.ConfidenceScore,
                explanation = result.Explanation
            });
        }


        // Ask a question to the AI system about energy optimization
        [HttpPost("ask")]
        public async Task<ActionResult<object>> AskQuestion([FromBody] AskQuestionRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Question)) return BadRequest(new { error = "Question is required" });

            var answer = await _geminiService.AskQuestion(request.Question, request.Context ?? "Energy optimization system");
            return Ok(new { question = request.Question, answer });

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
            var analyses = await _analysisRepo.ListAllAsync();
            var query = analyses.AsEnumerable();

            // Apply filters
            if (!string.IsNullOrEmpty(analysisType))
                query = query.Where(a => a.AnalysisType == analysisType);

            if (!string.IsNullOrEmpty(startDate) && DateTime.TryParse(startDate, out var start))
                query = query.Where(a => a.AnalysisDate >= start);

            if (!string.IsNullOrEmpty(endDate) && DateTime.TryParse(endDate, out var end))
                query = query.Where(a => a.AnalysisDate <= end.AddDays(1));

            var total = query.Count();
            var totalPages = (int)Math.Ceiling(total / (double)pageSize);

            var result = query
                .OrderByDescending(a => a.AnalysisDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new
                {
                    a.Id,
                    a.AnalysisType,
                    a.AnalysisDate,
                    a.Summary,
                    Period = new
                    {
                        Start = a.PeriodStart,
                        End = a.PeriodEnd
                    },
                    a.TotalConsumptionKWh,
                    a.DevicesAnalyzed
                })
                .ToList();

            return Ok(new
            {
                page,
                pageSize,
                totalPages,
                count = total,
                data = result
            });
        }

        // Get detailed analysis by ID
        [HttpGet("analysis/{id}")]
        public async Task<ActionResult<object>> GetAnalysisById(int id)
        {
            var analysis = await _analysisRepo.GetByIdAsync(id);

            if (analysis == null) return NotFound(new { error = $"Analysis {id} not found" });

            return Ok(analysis);
        }

        [HttpGet("Statistics")]
        public async Task<ActionResult<object>> GetAIStatistics()
        {
            var analyses = await _analysisRepo.ListAllAsync();
            var recommendations = await _recommendationRepo.ListAllAsync();
            var anomalies = await _anomalyRepo.ListAllAsync();
            var devices = await _deviceRepo.ListAllAsync();

            var stats = new
            {
                Analyses = new
                {
                    Total = analyses.Count(),
                    Last30Days = analyses.Count(a => a.AnalysisDate >= DateTime.UtcNow.AddDays(-30)),
                    ByType = analyses.GroupBy(a => a.AnalysisType)
                                   .Select(g => new { Type = g.Key, Count = g.Count() })
                },
                Recommendations = new
                {
                    Total = recommendations.Count(),
                    Active = recommendations.Count(r => !r.IsImplemented && (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)),
                    Implemented = recommendations.Count(r => r.IsImplemented),
                    TotalPotentialSavings = Math.Round(recommendations.Where(r => !r.IsImplemented).Sum(r => (double)r.EstimatedSavingsKWh), 2)
                },
                Anomalies = new
                {
                    Total = anomalies.Count(),
                    Unresolved = anomalies.Count(a => !a.IsResolved),
                    Resolved = anomalies.Count(a => a.IsResolved),
                    Last7Days = anomalies.Count(a => a.DetectedAt >= DateTime.UtcNow.AddDays(-7)),
                    bySeverity = anomalies.GroupBy(a => a.Severity).Select(g => new { severity = g.Key, count = g.Count() }).ToList(),
                    DevicesAffected = anomalies.Where(a => !a.IsResolved).Select(a => a.DeviceId).Distinct().Count()
                }
            };

            return Ok(stats);
        }


        #endregion


        #region Recommendations Endpoints
        [HttpGet("recommendations")]
        public async Task<ActionResult<object>> GetRecommendations(
        [FromQuery] bool? isImplemented = null)
        {
            var recommendations = await _recommendationRepo.ListAllAsync();
            var query = recommendations.AsEnumerable();
            if (isImplemented.HasValue)
                query = query.Where(r => r.IsImplemented == isImplemented.Value);

            var result = query.OrderByDescending(r => r.Priority).ToList();

            return Ok(result);

        }


        [HttpPatch("recommendations/{id}/implement")]
        public async Task<ActionResult<object>> ImplementRecommendation(int id)
        {
            var rec = await _recommendationRepo.GetByIdAsync(id);
            if (rec == null) return NotFound(new { error = "Not found" });

            rec.IsImplemented = true;
            rec.ImplementedDate = DateTime.UtcNow;

            _recommendationRepo.Update(rec);
            await _recommendationRepo.SaveChangesAsync();

            return Ok(new { message = "Marked as implemented" });
        }

        [HttpDelete("recommndations/{id}")]
        public async Task<ActionResult<object>> DeleteRecommendation(int id)
        {
            var rec = await _recommendationRepo.GetByIdAsync(id);
            if (rec == null) return NotFound();

            _recommendationRepo.Delete(rec);
            await _recommendationRepo.SaveChangesAsync();
            return Ok(new { message = "Deleted successfully" });
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
            var anomaliesList = await _anomalyRepo.ListAllAsync();
            var devices = await _deviceRepo.ListAllAsync();
            var query = anomaliesList.AsEnumerable();

            if (isResolved.HasValue)
                query = query.Where(a => a.IsResolved == isResolved.Value);

            if (!string.IsNullOrEmpty(severity))
                query = query.Where(a => a.Severity == severity);

            if (deviceId.HasValue)
                query = query.Where(a => a.DeviceId == deviceId.Value);

            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Calculate statistics from ALL anomalies (not filtered)
            var allAnomalies = anomaliesList.AsEnumerable();
            var statistics = new
            {
                critical = allAnomalies.Count(a => a.Severity?.ToLower() == "critical"),
                high = allAnomalies.Count(a => a.Severity?.ToLower() == "high"),
                medium = allAnomalies.Count(a => a.Severity?.ToLower() == "medium"),
                low = allAnomalies.Count(a => a.Severity?.ToLower() == "low"),
                unresolved = allAnomalies.Count(a => !a.IsResolved),
                resolved = allAnomalies.Count(a => a.IsResolved),
                devicesAffected = allAnomalies.Where(a => !a.IsResolved).Select(a => a.DeviceId).Distinct().Count()
            };

            var result = query
                .OrderByDescending(a => a.DetectedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => {
                    var device = devices.FirstOrDefault(d => d.Id == a.DeviceId);
                    var deviationPercent = a.ExpectedValue != 0
                        ? ((a.ActualValue - a.ExpectedValue) / a.ExpectedValue) * 100
                        : 0;

                    return new
                    {
                        a.Id,
                        a.DeviceId,
                        a.AnomalyTimestamp,
                        actualValue = (double)a.ActualValue,
                        expectedValue = (double)a.ExpectedValue,
                        a.Severity,
                        a.Description,
                        a.IsResolved,
                        a.DetectedAt,
                        device = device != null ? new
                        {
                            name = device.Name,
                            zone = device.Zone?.Name ?? "Unknown"
                        } : null,
                        deviationPercent = Math.Round(deviationPercent, 1)
                    };
                });
            return Ok(new
            {
                page,
                pageSize,
                totalCount,
                totalPages,
                statistics,
                data = result
            });
        }
        [HttpGet("anomalies/{id}")]
        public async Task<ActionResult<object>> GetAnomalyById(int id)
        {
            var anomaly = await _anomalyRepo.GetByIdAsync(id);

            if (anomaly == null)
                return NotFound(new { error = $"Anomaly with ID {id} not found" });

            return Ok(anomaly);
        }

        [HttpPatch("anomalies/{id}/resolve")]
        public async Task<ActionResult<object>> ResolveAnomaly(
           int id,
           [FromBody] ResolveAnomalyRequest request)
        {
            var anomaly = await _anomalyRepo.GetByIdAsync(id);
            if (anomaly == null) return NotFound(new { error = "Anomaly not found" });

            anomaly.IsResolved = true;
            anomaly.ResolvedAt = DateTime.UtcNow;
            anomaly.ResolutionNotes = request.ResolutionNotes;

            _anomalyRepo.Update(anomaly);
            await _anomalyRepo.SaveChangesAsync();

            return Ok(new { message = "Anomaly resolved successfully" });
        }

        [HttpDelete("anomalies/{id}")]
        public async Task<ActionResult<object>> DeleteAnomaly(int id)
        {
            var anomaly = await _anomalyRepo.GetByIdAsync(id);
            if (anomaly == null) return NotFound();

            _anomalyRepo.Delete(anomaly);
            await _anomalyRepo.SaveChangesAsync();
            return Ok(new { message = "Deleted successfully" });
        }

        #endregion



        [HttpGet("test-connection")]
        public async Task<ActionResult<object>> TestConnection()
        {
            _logger.LogInformation("Testing Gemini API connection");

            var result = await _geminiService.AskQuestion(
                "Say 'Hello from Gemini!' if you can read this.",
                "This is a connection test");

            return Ok(new
            {
                success = true,
                message = "Connection successful",
                response = result
            });
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