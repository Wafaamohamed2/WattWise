using EnergyOptimizer.Core.Entities;
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

            
        }
    }
}
