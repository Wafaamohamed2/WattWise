using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EnergyOptimizer.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceTemplatesAndBuildingOnboarding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOnboardingComplete",
                table: "Buildings",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE Buildings SET IsOnboardingComplete = 1;");

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Buildings",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "DeviceTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BuildingType = table.Column<int>(type: "int", nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    SuggestedName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DefaultQuantity = table.Column<int>(type: "int", nullable: false),
                    DefaultRatedPowerKW = table.Column<double>(type: "float", nullable: false),
                    SuggestedZoneName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceTemplates", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DeviceTemplates_BuildingType",
                table: "DeviceTemplates",
                column: "BuildingType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeviceTemplates");

            migrationBuilder.DropColumn(
                name: "IsOnboardingComplete",
                table: "Buildings");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Buildings");
        }
    }
}
