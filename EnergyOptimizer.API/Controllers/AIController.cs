using EnergyOptimizer.API.Services;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Infrastructure.Data;
using EnergyOptimizer.Service.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;


namespace EnergyOptimizer.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AIController : ControllerBase
    {

        private readonly PatternDetectionService _patternService;
        private readonly IGeminiService _geminiService;
        private readonly EnergyDbContext _context;
        private readonly ILogger<AIController> _logger;

        public AIController(
            PatternDetectionService patternService,
            IGeminiService geminiService,
            EnergyDbContext context,
            ILogger<AIController> logger)
        {
            _patternService = patternService;
            _geminiService = geminiService;
            _context = context;
            _logger = logger;
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
                var analysis = new EnergyAnalysis
                {
                    AnalysisDate = DateTime.UtcNow,
                    AnalysisType = "Pattern",
                    PeriodStart = start,
                    PeriodEnd = end,
                    Summary = result.Summary,
                    FullResponse = JsonSerializer.Serialize(result)
                };

                _context.EnergyAnalyses.Add(analysis);
                await _context.SaveChangesAsync();




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

                _logger.LogInformation("Generating recommendations from {Start} to {End}", start, end);

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
                _context.EnergyAnalyses.Add(analysis);
                await _context.SaveChangesAsync();

                foreach (var rec in result.Recommendations)
                {
                    _context.EnergyRecommendations.Add(new EnergyRecommendation
                    {
                        Title = rec.Title,
                        Description = rec.Description,
                        Category = rec.Category,
                        Priority = rec.Priority,
                        EstimatedSavingsKWh = rec.PotentialSavingsKWh,
                        AnalysisId = analysis.Id
                    });

                }

                await _context.SaveChangesAsync();

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
                    totalRecommendations = result.Recommendations.Count,
                    estimatedSavings = new
                    {
                        kWh = result.EstimatedSavingsKWh,
                        percent = result.EstimatedSavingsPercent
                    }
                });
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


        // Ask a question to the AI system about energy optimization
        [HttpPost("ask")]
        public async Task<ActionResult<object>> AskQuestion([FromBody] AskQuestionRequest request)
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
        #endregion

        #region Analysis Endpoints
        // Endpoints for managing analysis history, recommendations, and anomalies
        [HttpGet("analysis-history")]
        public async Task<ActionResult<object>> GetAnalysisHistory(
            [FromQuery] string? analysisType = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {

                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();

                var query = context.EnergyAnalyses
                    .Include(a => a.Insights)
                    .Include(a => a.Recommendations)
                    .Include(a => a.Anomalies)
                    .AsQueryable();

                // Apply filters
                if (!string.IsNullOrEmpty(analysisType))
                {
                    query = query.Where(a => a.AnalysisType == analysisType);
                }

                if (startDate.HasValue)
                {
                    query = query.Where(a => a.AnalysisDate >= startDate.Value);
                }

                if (endDate.HasValue)
                {
                    query = query.Where(a => a.AnalysisDate <= endDate.Value);
                }

                // Get total count
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                // Get paginated results
                var analyses = await query
                    .OrderByDescending(a => a.AnalysisDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new
                    {
                        a.Id,
                        a.AnalysisDate,
                        a.AnalysisType,
                        a.Summary,
                        Period = new
                        {
                            Start = a.PeriodStart,
                            End = a.PeriodEnd
                        },
                        a.TotalConsumptionKWh,
                        a.DevicesAnalyzed,
                        InsightsCount = a.Insights.Count,
                        RecommendationsCount = a.Recommendations.Count,
                        AnomaliesCount = a.Anomalies.Count,
                        Insights = a.Insights.Select(i => new
                        {
                            i.Id,
                            i.InsightText,
                            i.Category,
                            i.Priority
                        }).ToList()
                    })
                    .ToListAsync();

                return Ok(new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    data = analyses
                });
        }

        // Get detailed analysis by ID
        [HttpGet("analysis/{id}")]
        public async Task<ActionResult<object>> GetAnalysisById(int id)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();

                var analysis = await context.EnergyAnalyses
                    .Include(a => a.Insights)
                    .Include(a => a.Recommendations)
                    .Include(a => a.Anomalies)
                        .ThenInclude(an => an.Device)
                    .Where(a => a.Id == id)
                    .Select(a => new
                    {
                        a.Id,
                        a.AnalysisDate,
                        a.AnalysisType,
                        a.Summary,
                        a.FullResponse,
                        Period = new
                        {
                            Start = a.PeriodStart,
                            End = a.PeriodEnd
                        },
                        a.TotalConsumptionKWh,
                        a.DevicesAnalyzed,
                        Insights = a.Insights.OrderBy(i => i.Priority).Select(i => new
                        {
                            i.Id,
                            i.InsightText,
                            i.Category,
                            Priority = i.Priority == 1 ? "High" : i.Priority == 2 ? "Medium" : "Low",
                            i.CreatedAt
                        }).ToList(),
                        Recommendations = a.Recommendations.Select(r => new
                        {
                            r.Id,
                            r.Title,
                            r.Description,
                            r.Category,
                            r.Priority,
                            r.EstimatedSavingsKWh,
                            r.IsImplemented,
                            r.CreatedAt
                        }).ToList(),
                        Anomalies = a.Anomalies.Select(an => new
                        {
                            an.Id,
                            Device = new
                            {
                                an.Device.Id,
                                an.Device.Name
                            },
                            an.AnomalyTimestamp,
                            an.ActualValue,
                            an.ExpectedValue,
                            an.Deviation,
                            an.Severity,
                            an.Description,
                            an.IsResolved
                        }).ToList()
                    })
                        .FirstOrDefaultAsync();

                if (analysis == null)
                {
                    return NotFound(new { error = $"Analysis with ID {id} not found" });
                }
                return Ok(analysis);
        }

        [HttpGet("Statistics")]
        public async Task<ActionResult<object>> GetAIStatistics()
        { 
                var stats = new
                {
                    Analyses = new
                    {
                        Total = await _context.EnergyAnalyses.CountAsync(),
                        Last30Days = await _context.EnergyAnalyses
                           .CountAsync(a => a.AnalysisDate >= DateTime.UtcNow.AddDays(-30)),
                        ByType = await _context.EnergyAnalyses
                           .GroupBy(a => a.AnalysisType)
                           .Select(g => new { Type = g.Key, Count = g.Count() })
                           .ToListAsync()
                    },
                    Recommendations = new
                    {
                        Total = await _context.EnergyRecommendations.CountAsync(),
                        Active = await _context.EnergyRecommendations
                            .CountAsync(r => !r.IsImplemented && (r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)),
                        Implemented = await _context.EnergyRecommendations.CountAsync(r => r.IsImplemented),
                        TotalPotentialSavings = Math.Round(
                              (await _context.EnergyRecommendations
                                 .Where(r => !r.IsImplemented)
                                 .Select(r => r.EstimatedSavingsKWh)
                                 .ToListAsync())
                                 .Sum(), 2),
                        ByPriority = await _context.EnergyRecommendations
                            .Where(r => !r.IsImplemented)
                            .GroupBy(r => r.Priority)
                            .Select(g => new { Priority = g.Key, Count = g.Count() })
                            .ToListAsync()
                    },
                    Anomalies = new
                    {
                        Total = await _context.DetectedAnomalies.CountAsync(),
                        Unresolved = await _context.DetectedAnomalies.CountAsync(a => !a.IsResolved),
                        Resolved = await _context.DetectedAnomalies.CountAsync(a => a.IsResolved),
                        Last7Days = await _context.DetectedAnomalies
                            .CountAsync(a => a.DetectedAt >= DateTime.UtcNow.AddDays(-7)),
                        BySeverity = await _context.DetectedAnomalies
                            .Where(a => !a.IsResolved)
                            .GroupBy(a => a.Severity)
                            .Select(g => new { Severity = g.Key, Count = g.Count() })
                            .ToListAsync(),
                        DevicesAffected = await _context.DetectedAnomalies
                            .Where(a => !a.IsResolved)
                            .Select(a => a.DeviceId)
                            .Distinct()
                            .CountAsync()
                    },
                    LastAnalysis = await _context.EnergyAnalyses
                        .OrderByDescending(a => a.AnalysisDate)
                        .Select(a => new
                        {
                            a.Id,
                            a.AnalysisDate,
                            a.AnalysisType,
                            a.Summary
                        })
                        .FirstOrDefaultAsync()
                };

                return Ok(stats);
        }


        #endregion


        #region Recommendations Endpoints
        [HttpGet("recommendations")]
        public async Task<ActionResult<object>> GetRecommendations(
        [FromQuery] bool? isImplemented = null,
        [FromQuery] string? priority = null,
        [FromQuery] string? category = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();

                var query = _context.EnergyRecommendations
                    .Where(r => r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)
                    .AsQueryable();

                // Apply filters
                if (isImplemented.HasValue)
                {
                    query = query.Where(r => r.IsImplemented == isImplemented.Value);
                }

                if (!string.IsNullOrEmpty(priority))
                {
                    query = query.Where(r => r.Priority == priority);
                }

                if (!string.IsNullOrEmpty(category))
                {
                    query = query.Where(r => r.Category == category);
                }

                // Get total count
                var totalCount = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

                var recommendationsList = await query
                    .OrderByDescending(r => r.Priority == "High" ? 1 : r.Priority == "Medium" ? 2 : 3)
                    .ThenByDescending(r => r.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(); 

                var recommendations = recommendationsList.Select(r => new
                {
                    r.Id,
                    r.Title,
                    r.Description,
                    r.Category,
                    r.Priority,
                    EstimatedSavingsKWh = (double)r.EstimatedSavingsKWh, 
                    EstimatedSavingsPercent = (double)r.EstimatedSavingsPercent, 
                    r.IsImplemented,
                    r.CreatedAt
                }).ToList();

      
                var allRecommendations = await _context.EnergyRecommendations
                    .Where(r => r.ExpiresAt == null || r.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync(); 

                var stats = new
                {
                    Total = allRecommendations.Count,
                    Implemented = allRecommendations.Count(r => r.IsImplemented),
                    Pending = allRecommendations.Count(r => !r.IsImplemented),
                    HighPriority = allRecommendations.Count(r => !r.IsImplemented && r.Priority == "High"),
                    TotalEstimatedSavings = (double)allRecommendations
                        .Where(r => !r.IsImplemented)
                        .Sum(r => r.EstimatedSavingsKWh) 
                };

                return Ok(new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    statistics = stats,
                    data = recommendations
                });
        }

    
        [HttpPatch("recommendations/{id}/implement")]
        public async Task<ActionResult<object>> ImplementRecommendation(int id)
        {
            var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();
                var recommendation = await context.EnergyRecommendations.FindAsync(id);

                if (recommendation == null)
                {
                    return NotFound(new { error = $"Recommendation with ID {id} not found" });
                }

                if (recommendation.IsImplemented)
                {
                    return BadRequest(new { error = "Recommendation is already implemented" });
                }

                recommendation.IsImplemented = true;
                recommendation.ImplementedDate = DateTime.UtcNow;
                await context.SaveChangesAsync();

                _logger.LogInformation("Recommendation {Id} marked as implemented", id);

                return Ok(new
                {
                    id = recommendation.Id,
                    title = recommendation.Title,
                    isImplemented = recommendation.IsImplemented,
                    implementedDate = recommendation.ImplementedDate,
                    message = "Recommendation marked as implemented successfully"
                });
        }

        [HttpDelete("recommndations/{id}")]
        public async Task<ActionResult<object>> DeleteRecommendation(int id)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();
                var recommendation = await context.EnergyRecommendations.FindAsync(id);
                if (recommendation == null)
                {
                    return NotFound(new { error = $"Recommendation with ID {id} not found" });
                }

                context.EnergyRecommendations.Remove(recommendation);
                await context.SaveChangesAsync();
                return Ok(new
                {
                    id,
                    message = "Recommendation deleted successfully"
                });
        }
        #endregion


        #region Anomalies Endpoints
        [HttpGet("anomalies")]
        public async Task<ActionResult<object>> GetAnomalies(
        [FromQuery] bool? isResolved = null,
        [FromQuery] string? severity = null,
        [FromQuery] int? deviceId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
        {

                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();

                var query = context.DetectedAnomalies
                    .AsNoTracking()
                    .Include(a => a.Device)
                        .ThenInclude(d => d.Zone)
                    .AsQueryable();

                // Apply filters
                if (isResolved.HasValue)
                {
                    query = query.Where(a => a.IsResolved == isResolved.Value);
                }
                if (!string.IsNullOrEmpty(severity))
                {
                    query = query.Where(a => a.Severity == severity);
                }
                if (deviceId.HasValue)
                {
                    query = query.Where(a => a.DeviceId == deviceId.Value);
                }
                if (startDate.HasValue)
                {
                    query = query.Where(a => a.DetectedAt >= startDate.Value);
                }
                if (endDate.HasValue)
                {
                    query = query.Where(a => a.DetectedAt <= endDate.Value);
                }

                var totalCount = await query.CountAsync();

                // Fetch to memory first
                var anomaliesList = await query
                    .OrderByDescending(a => a.Severity == "Critical" ? 1 : a.Severity == "High" ? 2 : a.Severity == "Medium" ? 3 : 4)
                    .ThenByDescending(a => a.DetectedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(); // Get from DB first

                // in memory with explicit casts
                var anomalies = anomaliesList.Select(a => new
                {
                    a.Id,
                    Device = new
                    {
                        a.Device.Id,
                        a.Device.Name,
                        Zone = a.Device.Zone.Name ?? "Unknown"
                    },
                    a.AnomalyTimestamp,
                    ActualValue = (double)a.ActualValue,     
                    ExpectedValue = (double)a.ExpectedValue,  
                    Deviation = (double)a.Deviation,          
                    DeviationPercent = a.ExpectedValue > 0
                        ? Math.Round(((double)a.Deviation / (double)a.ExpectedValue) * 100, 2)
                        : 0,
                    a.Severity,
                    a.Description,
                    a.IsResolved,
                    a.ResolutionNotes,
                    a.ResolvedAt,
                    a.DetectedAt,
                    a.PossibleCauses
                }).ToList();

                var unresolvedQuery = context.DetectedAnomalies.Where(a => !a.IsResolved);

                var stats = new
                {
                    Total = totalCount,
                    Unresolved = await unresolvedQuery.CountAsync(),
                    Resolved = await query.CountAsync(a => a.IsResolved),
                    Critical = await unresolvedQuery.CountAsync(a => a.Severity == "Critical"),
                    High = await unresolvedQuery.CountAsync(a => a.Severity == "High"),
                    Medium = await unresolvedQuery.CountAsync(a => a.Severity == "Medium"),
                    DevicesAffected = await unresolvedQuery
                            .Select(a => a.DeviceId)
                            .Distinct()
                            .CountAsync()
                };

                var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
                return Ok(new
                {
                    page,
                    pageSize,
                    totalCount,
                    totalPages,
                    statistics = stats,
                    data = anomalies
                });
        }
        [HttpGet("anomalies/{id}")]
        public async Task<ActionResult<object>> GetAnomalyById(int id)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();
                var anomaly = await context.DetectedAnomalies
                    .Include(a => a.Device)
                        .ThenInclude(d => d.Zone)
                        .Where(a => a.Id == id)
                        .Select(a => new
                        {
                            a.Id,
                            Device = new
                            {
                                a.Device.Id,
                                a.Device.Name,
                                Zone = new
                                {
                                    a.Device.Zone.Id,
                                    a.Device.Zone.Name
                                }
                            },
                            a.AnomalyTimestamp,
                            a.ActualValue,
                            a.ExpectedValue,
                            a.Deviation,
                            DeviationPercent = a.ExpectedValue > 0
                            ? Math.Round((a.Deviation / a.ExpectedValue) * 100, 2)
                            : 0,
                            a.Severity,
                            a.Description,
                            a.IsResolved,
                            a.ResolutionNotes
                        })
                    .FirstOrDefaultAsync();

                if (anomaly == null)
                {
                    return NotFound(new { error = $"Anomaly with ID {id} not found" });
                }

                return Ok(anomaly);
        }

        [HttpPatch("anomalies/{id}/resolve")]
        public async Task<ActionResult<object>> ResolveAnomaly(
           int id,
           [FromBody] ResolveAnomalyRequest request)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();
                var anomaly = await context.DetectedAnomalies.FindAsync(id);

                if (anomaly == null)
                {
                    return NotFound(new { error = $"Anomaly with ID {id} not found" });
                }
                if (anomaly.IsResolved)
                {
                    return BadRequest(new { error = "Anomaly is already resolved" });
                }

                anomaly.IsResolved = true;
                anomaly.ResolvedAt = DateTime.UtcNow;
                anomaly.ResolutionNotes = request.ResolutionNotes;

                await context.SaveChangesAsync();

                _logger.LogInformation("Anomaly {Id} marked as resolved", id);

                return Ok(new
                {
                    id = anomaly.Id,
                    isResolved = anomaly.IsResolved,
                    resolvedAt = anomaly.ResolvedAt,
                    resolutionNotes = anomaly.ResolutionNotes,
                    message = "Anomaly marked as resolved successfully"
                });
        }

        [HttpDelete("anomalies/{id}")]
        public async Task<ActionResult<object>> DeleteAnomaly(int id)
        {
                var context = HttpContext.RequestServices.GetRequiredService<EnergyDbContext>();
                var anomaly = await context.DetectedAnomalies.FindAsync(id);
                if (anomaly == null)
                {
                    return NotFound(new { error = $"Anomaly with ID {id} not found" });
                }

                context.DetectedAnomalies.Remove(anomaly);
                await context.SaveChangesAsync();
                return Ok(new
                {
                    id,
                    message = "Anomaly deleted successfully"
                });

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