using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class ProfileDisplayName2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ratings",
                table: "CardGames");

            migrationBuilder.AddColumn<double>(
                name: "AvgRating",
                table: "CardGames",
                type: "float(3)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RatingsCount",
                table: "CardGames",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "AvgRating",
                table: "BoardGames",
                type: "float(3)",
                precision: 3,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "RatingsCount",
                table: "BoardGames",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvgRating",
                table: "CardGames");

            migrationBuilder.DropColumn(
                name: "RatingsCount",
                table: "CardGames");

            migrationBuilder.DropColumn(
                name: "AvgRating",
                table: "BoardGames");

            migrationBuilder.DropColumn(
                name: "RatingsCount",
                table: "BoardGames");

            migrationBuilder.AddColumn<string>(
                name: "Ratings",
                table: "CardGames",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
