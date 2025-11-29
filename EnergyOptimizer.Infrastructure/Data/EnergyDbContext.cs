using EnergyOptimizer.Core.Entities;
using EnergyOptimizer.Core.Entities.AI_Analysis;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnergyOptimizer.Infrastructure.Data
{
    public class EnergyDbContext : DbContext
    {
        public EnergyDbContext(DbContextOptions<EnergyDbContext> options) : base(options)
        {
        }

        #region DbSets
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Zone> Zones { get; set; }
        public DbSet<Device> Devices { get; set; }
        public DbSet<EnergyReading> EnergyReadings { get; set; }
        public DbSet<Alert> Alerts { get; set; }

        // AI/ML Entities 
        public DbSet<EnergyAnalysis> EnergyAnalyses { get; set; }
        public DbSet<AnalysisInsight> AnalysisInsights { get; set; }
        public DbSet<EnergyRecommendation> EnergyRecommendations { get; set; }
        public DbSet<DetectedAnomaly> DetectedAnomalies { get; set; }
        public DbSet<ConsumptionPrediction> ConsumptionPredictions { get; set; }
        public DbSet<UsagePattern> UsagePatterns { get; set; }
        public DbSet<AIMetrics> AIMetrics { get; set; }

        #endregion
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Building Configuration
            modelBuilder.Entity<Building>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Address).HasMaxLength(500);

                entity.HasMany(e => e.Zones)
                      .WithOne(e => e.Building)
                      .HasForeignKey(e => e.BuildingId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Zone Configuration
            modelBuilder.Entity<Zone>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasConversion<int>();

                entity.HasMany(e => e.Devices)
                      .WithOne(e => e.Zone)
                      .HasForeignKey(e => e.ZoneId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Device Configuration
            modelBuilder.Entity<Device>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Type).HasConversion<int>();
                entity.Property(e => e.RatedPowerKW).HasColumnType("decimal(18,2)");

                entity.HasMany(e => e.EnergyReadings)
                      .WithOne(e => e.Device)
                      .HasForeignKey(e => e.DeviceId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // EnergyReading Configuration
            modelBuilder.Entity<EnergyReading>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.PowerConsumptionKW).HasColumnType("decimal(18,4)");
                entity.Property(e => e.Voltage).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Current).HasColumnType("decimal(18,2)");
                entity.Property(e => e.Temperature).HasColumnType("decimal(18,2)");

                entity.HasIndex(e => e.Timestamp);
                entity.HasIndex(e => e.DeviceId);
            });

            // Alert Configuration
            modelBuilder.Entity<Alert>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Message).IsRequired().HasMaxLength(500);
                entity.Property(e => e.Type).HasConversion<int>();

                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.IsRead);
            });


            // EnergyAnalysis Configuration
            modelBuilder.Entity<EnergyAnalysis>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.AnalysisType).IsRequired().HasMaxLength(50);
                entity.Property(e => e.Summary).IsRequired();
                entity.Property(e => e.TotalConsumptionKWh).HasPrecision(12, 2);

                entity.HasMany(e => e.Insights)
                    .WithOne(i => i.Analysis)
                    .HasForeignKey(i => i.AnalysisId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasMany(e => e.Recommendations)
                    .WithOne(r => r.Analysis)
                    .HasForeignKey(r => r.AnalysisId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasMany(e => e.Anomalies)
                    .WithOne(a => a.Analysis)
                    .HasForeignKey(a => a.AnalysisId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasIndex(e => e.AnalysisDate);
                entity.HasIndex(e => e.AnalysisType);
                entity.HasIndex(e => new { e.PeriodStart, e.PeriodEnd });
            });

            // AnalysisInsight Configuration
            modelBuilder.Entity<AnalysisInsight>(entity =>
            {
                entity.HasKey(i => i.Id);
                entity.Property(i => i.InsightText).IsRequired();
                entity.Property(i => i.Category).HasMaxLength(50);
                entity.HasIndex(i => i.AnalysisId);
                entity.HasIndex(i => i.Priority);
            });

            // EnergyRecommendation Configuration
            modelBuilder.Entity<EnergyRecommendation>(entity =>
            {
                entity.HasKey(r => r.Id);
                entity.Property(r => r.Title).IsRequired().HasMaxLength(200);
                entity.Property(r => r.Description).IsRequired();
                entity.Property(r => r.Category).IsRequired().HasMaxLength(50);
                entity.Property(r => r.Priority).IsRequired().HasMaxLength(20);
                entity.Property(r => r.EstimatedSavingsKWh).HasPrecision(10, 2);
                entity.Property(r => r.EstimatedSavingsPercent).HasPrecision(5, 2);

                entity.HasIndex(r => r.CreatedAt);
                entity.HasIndex(r => r.IsImplemented);
                entity.HasIndex(r => new { r.Priority, r.IsImplemented });
            });

            // DetectedAnomaly Configuration
            modelBuilder.Entity<DetectedAnomaly>(entity =>
            {
                entity.HasKey(a => a.Id);
                entity.Property(a => a.Description).IsRequired();
                entity.Property(a => a.Severity).IsRequired().HasMaxLength(20);
                entity.Property(a => a.ActualValue)
               .HasColumnType("float")        
               .HasConversion<double>();     

                entity.Property(a => a.ExpectedValue)
                      .HasColumnType("float")
                      .HasConversion<double>();

                entity.Property(a => a.Deviation)
                      .HasColumnType("float")
                      .HasConversion<double>();

                entity.HasOne(a => a.Device)
                    .WithMany()
                    .HasForeignKey(a => a.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasIndex(a => a.DetectedAt);
                entity.HasIndex(a => a.DeviceId);
                entity.HasIndex(a => new { a.IsResolved, a.Severity });
            });

            // ConsumptionPrediction Configuration
            modelBuilder.Entity<ConsumptionPrediction>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PredictedConsumptionKWh).HasPrecision(12, 2);
                entity.Property(p => p.ActualConsumptionKWh).HasPrecision(12, 2);
                entity.Property(p => p.ConfidenceScore).HasPrecision(5, 4);
                entity.Property(p => p.PredictionType).HasMaxLength(50);

                entity.HasIndex(p => p.PredictionDate);
                entity.HasIndex(p => p.CreatedAt);
            });

            // UsagePattern Configuration
            modelBuilder.Entity<UsagePattern>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.PatternName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.PatternType).IsRequired().HasMaxLength(50);
                entity.Property(p => p.AverageConsumption).HasPrecision(10, 4);
                entity.Property(p => p.PeakConsumption).HasPrecision(10, 4);
                entity.Property(p => p.Confidence).HasPrecision(5, 4);

                entity.HasOne(p => p.Device)
                    .WithMany()
                    .HasForeignKey(p => p.DeviceId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(p => p.Zone)
                    .WithMany()
                    .HasForeignKey(p => p.ZoneId)
                    .OnDelete(DeleteBehavior.NoAction); 

                entity.HasIndex(p => p.DetectedAt);
                entity.HasIndex(p => new { p.DeviceId, p.IsActive });
                entity.HasIndex(p => new { p.ZoneId, p.IsActive });
            });


            // AIMetrics Configuration
            modelBuilder.Entity<AIMetrics>(entity =>
            {
                entity.HasKey(m => m.Id);
                entity.Property(m => m.MetricType).IsRequired().HasMaxLength(50);
                entity.Property(m => m.AverageResponseTimeMs).HasPrecision(10, 2);
                entity.Property(m => m.AverageCost).HasPrecision(10, 4);

                entity.HasIndex(m => m.Timestamp);
                entity.HasIndex(m => m.MetricType);
            });





        }
    }
}
