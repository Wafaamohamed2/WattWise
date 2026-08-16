using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSuggestedZoneTypeToDeviceTemplate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SuggestedZoneType",
                table: "DeviceTemplates",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SuggestedZoneType",
                table: "DeviceTemplates");
        }
    }
}
