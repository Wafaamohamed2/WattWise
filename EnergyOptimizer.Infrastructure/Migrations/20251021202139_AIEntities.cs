using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AIEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AIMetrics",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MetricType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TotalRequests = table.Column<int>(type: "int", nullable: false),
                    SuccessfulRequests = table.Column<int>(type: "int", nullable: false),
                    FailedRequests = table.Column<int>(type: "int", nullable: false),
                    AverageResponseTimeMs = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    AverageCost = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    CacheHits = table.Column<int>(type: "int", nullable: false),
                    CacheMisses = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIMetrics", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConsumptionPredictions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PredictionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PredictedConsumptionKWh = table.Column<double>(type: "float(12)", precision: 12, scale: 2, nullable: false),
                    ActualConsumptionKWh = table.Column<double>(type: "float(12)", precision: 12, scale: 2, nullable: true),
                    ConfidenceScore = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PredictionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumptionPredictions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EnergyAnalyses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnalysisType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FullResponse = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalConsumptionKWh = table.Column<double>(type: "float(12)", precision: 12, scale: 2, nullable: false),
                    DevicesAnalyzed = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyAnalyses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UsagePatterns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<int>(type: "int", nullable: true),
                    ZoneId = table.Column<int>(type: "int", nullable: true),
                    PatternName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PatternType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AverageConsumption = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    PeakConsumption = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    PeakHours = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Frequency = table.Column<int>(type: "int", nullable: false),
                    Confidence = table.Column<double>(type: "float(5)", precision: 5, scale: 4, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsagePatterns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsagePatterns_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsagePatterns_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AnalysisInsights",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisId = table.Column<int>(type: "int", nullable: false),
                    InsightText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnalysisInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AnalysisInsights_EnergyAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "EnergyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DetectedAnomalies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisId = table.Column<int>(type: "int", nullable: true),
                    DeviceId = table.Column<int>(type: "int", nullable: false),
                    DetectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AnomalyTimestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ActualValue = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    ExpectedValue = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    Deviation = table.Column<double>(type: "float(10)", precision: 10, scale: 4, nullable: false),
                    Severity = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PossibleCauses = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsResolved = table.Column<bool>(type: "bit", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ResolutionNotes = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetectedAnomalies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DetectedAnomalies_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetectedAnomalies_EnergyAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "EnergyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "EnergyRecommendations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnalysisId = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    EstimatedSavingsKWh = table.Column<double>(type: "float(10)", precision: 10, scale: 2, nullable: false),
                    EstimatedSavingsPercent = table.Column<double>(type: "float(5)", precision: 5, scale: 2, nullable: false),
                    ActionItems = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsImplemented = table.Column<bool>(type: "bit", nullable: false),
                    ImplementedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EnergyRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EnergyRecommendations_EnergyAnalyses_AnalysisId",
                        column: x => x.AnalysisId,
                        principalTable: "EnergyAnalyses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIMetrics_MetricType",
                table: "AIMetrics",
                column: "MetricType");

            migrationBuilder.CreateIndex(
                name: "IX_AIMetrics_Timestamp",
                table: "AIMetrics",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisInsights_AnalysisId",
                table: "AnalysisInsights",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_AnalysisInsights_Priority",
                table: "AnalysisInsights",
                column: "Priority");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionPredictions_CreatedAt",
                table: "ConsumptionPredictions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumptionPredictions_PredictionDate",
                table: "ConsumptionPredictions",
                column: "PredictionDate");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedAnomalies_AnalysisId",
                table: "DetectedAnomalies",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedAnomalies_DetectedAt",
                table: "DetectedAnomalies",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedAnomalies_DeviceId",
                table: "DetectedAnomalies",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_DetectedAnomalies_IsResolved_Severity",
                table: "DetectedAnomalies",
                columns: new[] { "IsResolved", "Severity" });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAnalyses_AnalysisDate",
                table: "EnergyAnalyses",
                column: "AnalysisDate");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAnalyses_AnalysisType",
                table: "EnergyAnalyses",
                column: "AnalysisType");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyAnalyses_PeriodStart_PeriodEnd",
                table: "EnergyAnalyses",
                columns: new[] { "PeriodStart", "PeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_EnergyRecommendations_AnalysisId",
                table: "EnergyRecommendations",
                column: "AnalysisId");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyRecommendations_CreatedAt",
                table: "EnergyRecommendations",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyRecommendations_IsImplemented",
                table: "EnergyRecommendations",
                column: "IsImplemented");

            migrationBuilder.CreateIndex(
                name: "IX_EnergyRecommendations_Priority_IsImplemented",
                table: "EnergyRecommendations",
                columns: new[] { "Priority", "IsImplemented" });

            migrationBuilder.CreateIndex(
                name: "IX_UsagePatterns_DetectedAt",
                table: "UsagePatterns",
                column: "DetectedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsagePatterns_DeviceId_IsActive",
                table: "UsagePatterns",
                columns: new[] { "DeviceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_UsagePatterns_ZoneId_IsActive",
                table: "UsagePatterns",
                columns: new[] { "ZoneId", "IsActive" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIMetrics");

            migrationBuilder.DropTable(
                name: "AnalysisInsights");

            migrationBuilder.DropTable(
                name: "ConsumptionPredictions");

            migrationBuilder.DropTable(
                name: "DetectedAnomalies");

            migrationBuilder.DropTable(
                name: "EnergyRecommendations");

            migrationBuilder.DropTable(
                name: "UsagePatterns");

            migrationBuilder.DropTable(
                name: "EnergyAnalyses");
        }
    }
}
