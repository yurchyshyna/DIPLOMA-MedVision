using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedVision.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddUserRelationAndPatientInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PatientBirthDate",
                table: "XrayAnalyses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientFullName",
                table: "XrayAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PatientGender",
                table: "XrayAnalyses",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "XrayAnalyses",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_XrayAnalyses_UserId",
                table: "XrayAnalyses",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_XrayAnalyses_Users_UserId",
                table: "XrayAnalyses",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_XrayAnalyses_Users_UserId",
                table: "XrayAnalyses");

            migrationBuilder.DropIndex(
                name: "IX_XrayAnalyses_UserId",
                table: "XrayAnalyses");

            migrationBuilder.DropColumn(
                name: "PatientBirthDate",
                table: "XrayAnalyses");

            migrationBuilder.DropColumn(
                name: "PatientFullName",
                table: "XrayAnalyses");

            migrationBuilder.DropColumn(
                name: "PatientGender",
                table: "XrayAnalyses");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "XrayAnalyses");
        }
    }
}
