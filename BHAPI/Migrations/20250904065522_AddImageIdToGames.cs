using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BoardHoardAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddImageIdToGames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "CardGames",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ImageId",
                table: "BoardGames",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "CardGames");

            migrationBuilder.DropColumn(
                name: "ImageId",
                table: "BoardGames");
        }
    }
}
