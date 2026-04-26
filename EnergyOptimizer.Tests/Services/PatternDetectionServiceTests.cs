using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using EnergyOptimizer.Core.Interfaces;
using EnergyOptimizer.Core.Enums;
using EnergyOptimizer.Service.Services;
using EnergyOptimizer.API.DTOs.Gemini;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using EnergyOptimizer.Service.Services.Abstract;
using Microsoft.EntityFrameworkCore.Query;

namespace EnergyOptimizer.Tests.Services
{
    using AnomalyEntity = EnergyOptimizer.Core.Entities.AI_Analysis.DetectedAnomaly;

    public class PatternDetectionServiceTests
    {
        private readonly Mock<IGenericRepository<EnergyReading>> _readingRepo;
        private readonly Mock<IGenericRepository<Device>> _deviceRepo;
        private readonly Mock<IGenericRepository<AnomalyEntity>> _anomalyRepo;
        private readonly Mock<IGenericRepository<EnergyRecommendation>> _recommendationRepo;
        private readonly Mock<IGenericRepository<ConsumptionPrediction>> _predictionRepo;
        private readonly Mock<IGenericRepository<EnergyAnalysis>> _analysisRepo;
        private readonly Mock<IGenericRepository<Alert>> _alertRepo;
        private readonly Mock<IGeminiService> _geminiService;
        private readonly Mock<ILogger<PatternDetectionService>> _logger;
        private readonly PatternDetectionService _service;

        public PatternDetectionServiceTests()
        {
            _readingRepo = new Mock<IGenericRepository<EnergyReading>>();
            _deviceRepo = new Mock<IGenericRepository<Device>>();
            _anomalyRepo = new Mock<IGenericRepository<AnomalyEntity>>();
            _recommendationRepo = new Mock<IGenericRepository<EnergyRecommendation>>();
            _predictionRepo = new Mock<IGenericRepository<ConsumptionPrediction>>();
            _analysisRepo = new Mock<IGenericRepository<EnergyAnalysis>>();
            _alertRepo = new Mock<IGenericRepository<Alert>>();
            _geminiService = new Mock<IGeminiService>();
            _logger = new Mock<ILogger<PatternDetectionService>>();

            _service = new PatternDetectionService(
                _readingRepo.Object,
                _deviceRepo.Object,
                _anomalyRepo.Object,
                _recommendationRepo.Object,
                _predictionRepo.Object,
                _analysisRepo.Object,
                _alertRepo.Object,
                _geminiService.Object,
                _logger.Object);
        }

        [Fact]
        public async Task AnalyzeConsumptionPatterns_NoReadings_ReturnsFailure()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-1);
            var endDate = DateTime.UtcNow;

            _readingRepo.Setup(r => r.GetQueryable())
                .Returns(GetQueryableMock(new List<EnergyReading>()));

            // Act
            var result = await _service.AnalyzeConsumptionPatterns(startDate, endDate);

            // Assert
            result.Success.Should().BeFalse();
            result.ErrorMessage.Should().Be("No data available for analysis");
        }

        [Fact]
        public async Task AnalyzeConsumptionPatterns_WithReadings_CallsGeminiAndSavesResults()
        {
            // Arrange
            var startDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddDays(1);
            
            var device = new Device { Id = 1, Name = "AC", Type = DeviceType.AirConditioner };
            var readings = new List<EnergyReading>
            {
                new EnergyReading { DeviceId = 1, Device = device, PowerConsumptionKW = 2.5m, Timestamp = startDate.AddHours(1) },
                new EnergyReading { DeviceId = 1, Device = device, PowerConsumptionKW = 3.0m, Timestamp = startDate.AddHours(2) }
            };

            _readingRepo.Setup(r => r.GetQueryable()).Returns(GetQueryableMock(readings));
            
            var aiResult = new GeminiAnalysisResult 
            { 
                Success = true, 
                Summary = "Good", 
                Insights = new List<string> { "Reduce AC use" } 
            };

            _geminiService.Setup(g => g.AnalyzeEnergyPatterns(It.IsAny<EnergyPatternData>()))
                .ReturnsAsync(aiResult);

            // Act
            var result = await _service.AnalyzeConsumptionPatterns(startDate, endDate);

            // Assert
            result.Success.Should().BeTrue();
            _geminiService.Verify(g => g.AnalyzeEnergyPatterns(It.IsNotNull<EnergyPatternData>()), Times.Once);
            _analysisRepo.Verify(r => r.Add(It.IsNotNull<EnergyAnalysis>()), Times.Once);
            _analysisRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DetectDeviceAnomalies_DeviceNotFound_ReturnsFalse()
        {
            // Arrange
            _deviceRepo.Setup(r => r.GetQueryable()).Returns(GetQueryableMock(new List<Device>()));

            // Act
            var result = await _service.DetectDeviceAnomalies(1);

            // Assert
            result.HasAnomalies.Should().BeFalse();
            result.Analysis.Should().Be("Device not found");
        }

        [Fact]
        public async Task GenerateRecommendations_WithData_CallsGemini()
        {
            // Arrange
            var startDate = DateTime.UtcNow.AddDays(-7);
            var endDate = DateTime.UtcNow;
            var readings = new List<EnergyReading> 
            { 
                new EnergyReading { Device = new Device { Name = "Fridge" }, PowerConsumptionKW = 1.0m, Timestamp = startDate.AddDays(1) } 
            };

            _readingRepo.Setup(r => r.GetQueryable()).Returns(GetQueryableMock(readings));
            _anomalyRepo.Setup(r => r.GetQueryable()).Returns(GetQueryableMock(new List<AnomalyEntity>()));
            _alertRepo.Setup(r => r.GetQueryable()).Returns(GetQueryableMock(new List<Alert>()));

            var recResult = new RecommendationResult { Recommendations = new List<Recommendation>() };
            _geminiService.Setup(g => g.GenerateRecommendations(It.IsAny<ConsumptionSummary>()))
                .ReturnsAsync(recResult);

            // Act
            var result = await _service.GenerateRecommendations(startDate, endDate);

            // Assert
            _geminiService.Verify(g => g.GenerateRecommendations(It.IsNotNull<ConsumptionSummary>()), Times.Once);
        }

        private static IQueryable<T> GetQueryableMock<T>(List<T> sourceList) where T : class
        {
            return new TestAsyncEnumerable<T>(sourceList);
        }

        private class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
        {
            private readonly IQueryProvider _inner;
            internal TestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;
            public IQueryable CreateQuery(Expression expression) => new TestAsyncEnumerable<TEntity>(expression);
            public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new TestAsyncEnumerable<TElement>(expression);
            public object Execute(Expression expression) => _inner.Execute(expression);
            public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);
            public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
            {
                var resultType = typeof(TResult).GetGenericArguments()[0];
                var result = _inner.Execute(expression);
                return (TResult)typeof(Task).GetMethod("FromResult").MakeGenericMethod(resultType).Invoke(null, new[] { result });
            }
        }

        private class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
        {
            public TestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
            public TestAsyncEnumerable(Expression expression) : base(expression) { }
            public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
            IQueryProvider IQueryable.Provider => new TestAsyncQueryProvider<T>(this);
        }

        private class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
        {
            private readonly IEnumerator<T> _inner;
            public TestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
            public T Current => _inner.Current;
            public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());
            public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(); }
        }
    }
}
