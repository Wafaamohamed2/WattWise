using EnergyOptimizer.API.DTOs.Gemini;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EnergyOptimizer.AI.Services
{
    public class GeminiService : IGeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<GeminiService> _logger;
        private readonly IMemoryCache _cache;
        private readonly GeminiSettings _settings;
        private readonly SemaphoreSlim _rateLimiter;
        private DateTime _lastRequestTime = DateTime.MinValue;

        public GeminiService(
            IHttpClientFactory httpClientFactory,
            ILogger<GeminiService> logger,
            IMemoryCache cache,
            IOptions<GeminiSettings> settings)
        {
            _httpClient = httpClientFactory.CreateClient();
            _logger = logger;
            _cache = cache;
            _settings = settings.Value;
            _rateLimiter = new SemaphoreSlim(1, 1);

            // ✅ Configure HttpClient
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<GeminiAnalysisResult> AnalyzeEnergyPatterns(EnergyPatternData data)
        {
            try
            {
                var cacheKey = $"pattern_analysis_{data.StartDate:yyyyMMdd}_{data.EndDate:yyyyMMdd}";

                if (_settings.EnableCaching && _cache.TryGetValue(cacheKey, out GeminiAnalysisResult? cached))
                {
                    _logger.LogInformation("Returning cached pattern analysis");
                    return cached!;
                }

                var prompt = BuildPatternAnalysisPrompt(data);
                var response = await CallGeminiAPI(prompt);

                var result = ParsePatternAnalysisResponse(response);

                if (_settings.EnableCaching && result.Success)
                {
                    _cache.Set(cacheKey, result, TimeSpan.FromMinutes(_settings.CacheDurationMinutes));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error analyzing energy patterns");
                return new GeminiAnalysisResult
                {
                    Success = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<AnomalyDetectionResult> DetectAnomalies(DeviceConsumptionData data)
        {
            try
            {
                var prompt = BuildAnomalyDetectionPrompt(data);
                var response = await CallGeminiAPI(prompt);

                return ParseAnomalyDetectionResponse(response, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error detecting anomalies for device {Device}", data.DeviceName);
                return new AnomalyDetectionResult
                {
                    HasAnomalies = false,
                    Analysis = $"Error: {ex.Message}"
                };
            }
        }

        public async Task<RecommendationResult> GenerateRecommendations(ConsumptionSummary summary)
        {
            try
            {
                var cacheKey = $"recommendations_{summary.PeriodStart:yyyyMMdd}";

                if (_settings.EnableCaching && _cache.TryGetValue(cacheKey, out RecommendationResult? cached))
                {
                    _logger.LogInformation("Returning cached recommendations");
                    return cached!;
                }

                var prompt = BuildRecommendationsPrompt(summary);
                var response = await CallGeminiAPI(prompt);

                var result = ParseRecommendationsResponse(response);

                if (_settings.EnableCaching)
                {
                    _cache.Set(cacheKey, result, TimeSpan.FromHours(12));
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating recommendations");
                return new RecommendationResult();
            }
        }

        public async Task<string> AskQuestion(string question, string context)
        {
            try
            {
                var prompt = $@"Context: {context}

Question: {question}

Please provide a clear, concise answer focused on energy optimization and efficiency.";

                return await CallGeminiAPI(prompt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing question");
                return "I'm sorry, I couldn't process your question at the moment.";
            }
        }

        public async Task<PredictionResult> PredictConsumption(HistoricalData data)
        {
            try
            {
                var prompt = BuildPredictionPrompt(data);
                var response = await CallGeminiAPI(prompt);

                return ParsePredictionResponse(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error predicting consumption");
                return new PredictionResult
                {
                    PredictionDate = DateTime.UtcNow.AddDays(1),
                    ConfidenceScore = 0,
                    Explanation = $"Error: {ex.Message}"
                };
            }
        }

        // ===== PRIVATE METHODS =====

        private async Task<string> CallGeminiAPI(string prompt)
        {
            await _rateLimiter.WaitAsync();
            try
            {
                // Validate configuration
                if (string.IsNullOrWhiteSpace(_settings.ApiKey))
                {
                    _logger.LogError("Gemini API Key is missing");
                    throw new InvalidOperationException(
                        "Gemini API Key is not configured. Please add 'Gemini:ApiKey' to appsettings.json. " +
                        "Get your API key from: https://aistudio.google.com/app/apikey");
                }

                // Manual rate limiting
                var timeSinceLastRequest = DateTime.UtcNow - _lastRequestTime;
                var minInterval = TimeSpan.FromMilliseconds(60000.0 / _settings.RateLimitPerMinute);
                if (timeSinceLastRequest < minInterval)
                {
                    await Task.Delay(minInterval - timeSinceLastRequest);
                }

                // Prepare request body
                var requestBody = new
                {
                    contents = new[]
                    {
                new { parts = new[] { new { text = prompt } } }
            },
                    generationConfig = new
                    {
                        temperature = _settings.Temperature,
                        maxOutputTokens = _settings.MaxTokens,
                        topP = 0.95,
                        topK = 40
                    }
                };

                var json = JsonSerializer.Serialize(requestBody, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                _logger.LogDebug("Gemini request payload: {Json}", json);

                var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_settings.Model}:generateContent?key={_settings.ApiKey}";
                _logger.LogInformation("Calling Gemini API: {Url}", url.Replace(_settings.ApiKey, "***KEY***"));

                var httpContent = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, httpContent);
                _lastRequestTime = DateTime.UtcNow;

                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogDebug("Gemini response ({Status}): {Body}", response.StatusCode, responseBody);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, responseBody);
                    throw new HttpRequestException($"Gemini API returned {response.StatusCode}");
                }

                if (string.IsNullOrWhiteSpace(responseBody))
                    throw new InvalidOperationException("Gemini returned an empty response");

                var jsonDoc = JsonDocument.Parse(responseBody);

                // Check for API-level error
                if (jsonDoc.RootElement.TryGetProperty("error", out var errorElement))
                {
                    var message = errorElement.TryGetProperty("message", out var msg) ? msg.GetString() : "Unknown error";
                    throw new InvalidOperationException($"Gemini API error: {message}");
                }

                // Safely extract text (fully protected against 2025+ API changes)
                if (!jsonDoc.RootElement.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Gemini returned no candidates: {Response}", responseBody);
                    throw new InvalidOperationException("No response generated by Gemini (possibly blocked by safety filters)");
                }

                var firstCandidate = candidates[0];

                if (!firstCandidate.TryGetProperty("content", out var contentElement))
                {
                    _logger.LogWarning("Missing 'content' in candidate");
                    throw new InvalidOperationException("Invalid response format: missing 'content'");
                }

                if (!contentElement.TryGetProperty("parts", out var parts) || parts.GetArrayLength() == 0)
                {
                    _logger.LogWarning("Missing 'parts' in content");
                    throw new InvalidOperationException("Invalid response format: missing 'parts'");
                }

                var firstPart = parts[0];
                if (!firstPart.TryGetProperty("text", out var textElement))
                {
                    _logger.LogWarning("Missing 'text' field in part");
                    throw new InvalidOperationException("Invalid response format: missing 'text'");
                }

                var result = textElement.GetString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(result))
                    throw new InvalidOperationException("Gemini returned empty text");

                _logger.LogInformation("Successfully received Gemini response ({Length} characters)", result.Length);
                return result;
            }
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                _logger.LogError(ex, "Unexpected error in CallGeminiAPI");
                throw new InvalidOperationException("Failed to get response from Gemini AI. Please try again later.", ex);
            }
            finally
            {
                _rateLimiter.Release();
            }
        }

        private string BuildPatternAnalysisPrompt(EnergyPatternData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Analyze the following energy consumption patterns:");
            sb.AppendLine();
            sb.AppendLine($"Period: {data.StartDate:yyyy-MM-dd} to {data.EndDate:yyyy-MM-dd}");
            sb.AppendLine($"Total Consumption: {data.TotalConsumptionKWh:F2} kWh");
            sb.AppendLine();
            sb.AppendLine("Device Patterns:");

            foreach (var device in data.DevicePatterns.Take(10))
            {
                sb.AppendLine($"- {device.DeviceName} ({device.DeviceType}):");
                sb.AppendLine($"  Average: {device.AverageConsumptionKWh:F2} kWh");
                sb.AppendLine($"  Peak: {device.PeakConsumptionKWh:F2} kWh");
                sb.AppendLine($"  Active Hours: {device.ActiveHours}");
                sb.AppendLine($"  Peak Hours: {string.Join(", ", device.PeakHours)}");
            }

            sb.AppendLine();
            sb.AppendLine("Please provide:");
            sb.AppendLine("1. A brief summary of consumption patterns");
            sb.AppendLine("2. Key insights (3-5 points)");
            sb.AppendLine("3. Specific recommendations for optimization");
            sb.AppendLine();
            sb.AppendLine("Format your response as:");
            sb.AppendLine("SUMMARY: [your summary]");
            sb.AppendLine("INSIGHTS:");
            sb.AppendLine("- [insight 1]");
            sb.AppendLine("- [insight 2]");
            sb.AppendLine("RECOMMENDATIONS:");
            sb.AppendLine("- [recommendation 1]");
            sb.AppendLine("- [recommendation 2]");

            return sb.ToString();
        }

        private string BuildAnomalyDetectionPrompt(DeviceConsumptionData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"Analyze the following consumption data for anomalies:");
            sb.AppendLine($"Device: {data.DeviceName}");
            sb.AppendLine($"Average Consumption: {data.AverageConsumption:F3} kWh");
            sb.AppendLine($"Standard Deviation: {data.StandardDeviation:F3} kWh");
            sb.AppendLine();
            sb.AppendLine("Recent readings:");

            foreach (var point in data.ConsumptionHistory.TakeLast(20))
            {
                sb.AppendLine($"{point.Timestamp:yyyy-MM-dd HH:mm}: {point.ConsumptionKWh:F3} kWh");
            }

            sb.AppendLine();
            sb.AppendLine("Identify any anomalies and explain potential causes.");

            return sb.ToString();
        }

        private string BuildRecommendationsPrompt(ConsumptionSummary summary)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Energy Consumption Summary:");
            sb.AppendLine($"Period: {summary.PeriodStart:yyyy-MM-dd} to {summary.PeriodEnd:yyyy-MM-dd}");
            sb.AppendLine($"Total: {summary.TotalConsumptionKWh:F2} kWh");
            sb.AppendLine($"Daily Average: {summary.AverageDailyConsumption:F2} kWh");
            sb.AppendLine();
            sb.AppendLine("Top Consumers:");

            foreach (var device in summary.DeviceSummaries.Take(5))
            {
                sb.AppendLine($"- {device.DeviceName}: {device.ConsumptionKWh:F2} kWh ({device.PercentageOfTotal:F1}%)");
            }

            if (summary.CurrentIssues.Any())
            {
                sb.AppendLine();
                sb.AppendLine("Current Issues:");
                foreach (var issue in summary.CurrentIssues)
                {
                    sb.AppendLine($"- {issue}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Provide 3-5 specific, actionable recommendations to reduce energy consumption.");

            return sb.ToString();
        }

        private string BuildPredictionPrompt(HistoricalData data)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Historical daily consumption data:");

            foreach (var day in data.DailyConsumptions.TakeLast(30))
            {
                sb.AppendLine($"{day.Date:yyyy-MM-dd}: {day.ConsumptionKWh:F2} kWh (Temp: {day.Temperature:F1}°C, {(day.IsWeekend ? "Weekend" : "Weekday")})");
            }

            sb.AppendLine();
            sb.AppendLine($"Based on these patterns, predict consumption for the next {data.DaysToPredict} days.");
            sb.AppendLine("Provide prediction with confidence level and explanation.");

            return sb.ToString();
        }

        private GeminiAnalysisResult ParsePatternAnalysisResponse(string response)
        {
            var result = new GeminiAnalysisResult { Success = true };

            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            string currentSection = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith("SUMMARY:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "SUMMARY";
                    result.Summary = trimmed.Substring(8).Trim();
                }
                else if (trimmed.StartsWith("INSIGHTS:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "INSIGHTS";
                }
                else if (trimmed.StartsWith("RECOMMENDATIONS:", StringComparison.OrdinalIgnoreCase))
                {
                    currentSection = "RECOMMENDATIONS";
                }
                else if (trimmed.StartsWith("-") || trimmed.StartsWith("•"))
                {
                    var text = trimmed.TrimStart('-', '•', ' ');
                    if (currentSection == "INSIGHTS")
                        result.Insights.Add(text);
                    else if (currentSection == "RECOMMENDATIONS")
                        result.Recommendations.Add(text);
                }
            }

            // If no structured format, use whole response as summary
            if (string.IsNullOrWhiteSpace(result.Summary))
            {
                result.Summary = response;
            }

            return result;
        }

        private AnomalyDetectionResult ParseAnomalyDetectionResponse(string response, DeviceConsumptionData data)
        {
            var hasAnomalies = response.ToLower().Contains("anomaly") ||
                             response.ToLower().Contains("unusual") ||
                             response.ToLower().Contains("spike");

            return new AnomalyDetectionResult
            {
                HasAnomalies = hasAnomalies,
                Analysis = response
            };
        }

        private RecommendationResult ParseRecommendationsResponse(string response)
        {
            var result = new RecommendationResult();
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Trim().StartsWith("-") || line.Trim().StartsWith("•"))
                {
                    var text = line.Trim().TrimStart('-', '•', ' ');
                    result.Recommendations.Add(new Recommendation
                    {
                        Title = text.Length > 50 ? text.Substring(0, 50) + "..." : text,
                        Description = text,
                        Priority = "Medium"
                    });
                }
            }

            return result;
        }

        private PredictionResult ParsePredictionResponse(string response)
        {
            return new PredictionResult
            {
                PredictionDate = DateTime.UtcNow.AddDays(1),
                PredictedConsumptionKWh = 0,
                ConfidenceScore = 0.7,
                Explanation = response
            };
        }
    }

    // Settings class
    public class GeminiSettings
    {
        public string ApiKey { get; set; } = string.Empty;
        public string ProjectId { get; set; } = string.Empty;
        public string Location { get; set; } = "us-central1";
        public string Model { get; set; } = "gemini-1.5-flash";
        public int MaxTokens { get; set; } = 1000;
        public double Temperature { get; set; } = 0.7;
        public int RateLimitPerMinute { get; set; } = 60;
        public bool EnableCaching { get; set; } = true;
        public int CacheDurationMinutes { get; set; } = 30;
    }
}