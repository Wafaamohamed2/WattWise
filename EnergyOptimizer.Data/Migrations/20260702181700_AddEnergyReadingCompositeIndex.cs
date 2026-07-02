using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEnergyReadingCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_EnergyReadings_DeviceId_Timestamp",
                table: "EnergyReadings",
                columns: new[] { "DeviceId", "Timestamp" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EnergyReadings_DeviceId_Timestamp",
                table: "EnergyReadings");
        }
    }
}
