using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedVision.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddHeatmapAndDetections : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DetectionsJson",
                table: "XrayAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HeatmapPath",
                table: "XrayAnalyses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetectionsJson",
                table: "XrayAnalyses");

            migrationBuilder.DropColumn(
                name: "HeatmapPath",
                table: "XrayAnalyses");
        }
    }
}
