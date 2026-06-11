using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedVision.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPreviewPath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewPath",
                table: "XrayAnalyses",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewPath",
                table: "XrayAnalyses");
        }
    }
}
