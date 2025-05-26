using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriWave.API.Migrations
{
    /// <inheritdoc />
    public partial class AddedCaloriesForSport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<float>(
                name: "CaloriesBurned",
                table: "SportLogs",
                type: "real",
                nullable: false,
                defaultValue: 0f);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CaloriesBurned",
                table: "SportLogs");
        }
    }
}
